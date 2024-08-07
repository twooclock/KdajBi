﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KdajBi.Web.Services
{
    public interface IApiTokenProvider
    {
        ApiSettings Settings();
        Task<JwtToken> GetToken(string email);
    }

    public class ApiSettings
    {
        public string BaseAddress { get; set; } //= "https://localhost:44338";
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LoginAddress { get; set; } // "/api/login"
        public string RefreshAddress { get; set; } // "/api/token/refresh"

    }

    public class JwtToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long Expiration { get; set; }
    }
    public class ApiTokenProvider : IApiTokenProvider
    {
        public readonly ApiSettings _apiSettings;
        protected readonly ILogger _logger;
        private ConcurrentDictionary<string, JwtToken> myTokens;
        public ApiTokenProvider(IOptions<ApiSettings> apiSettings, ILogger<ApiTokenProvider> logger)
        {
            _apiSettings = apiSettings.Value;
            _logger = logger;
            myTokens = new ConcurrentDictionary<string, JwtToken>();
        }


        public async Task<JwtToken> GetToken(string email)
        {
            try
            {

                if (myTokens.ContainsKey(email) == true)
                {
                    if (myTokens[email].Expiration == 0)
                    {
                        //request token
                        _logger.LogInformation("GetAPIToken.Expiration=0 -> RequestToken("+ email + ")");
                        myTokens[email] = await RequestToken(email);
                    }
                    else
                    {
                        if (DateTime.UtcNow.Ticks > myTokens[email].Expiration)
                        {
                            //refresh token if expired
                            _logger.LogInformation("GetAPIToken.Expired -> RefreshToken("+ email + ")");
                            myTokens[email] = await RefreshToken(email);
                        }
                    }
                }
                else {
                    _logger.LogInformation("GetAPIToken.FirstRequestToken("+ email + ")");
                    myTokens[email] = await RequestToken(email); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAPIToken", email);
                throw;
            }
            return myTokens[email];
        }

        public ApiSettings Settings()
        {
            return _apiSettings;
        }

        private async Task<JwtToken> RefreshToken(string email)
        {
            JwtToken fals = new JwtToken();
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_apiSettings.BaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + myTokens[email].AccessToken);
                string stringData = "{\"UserEmail\":\"" + email + "\",\"Token\":\"" + myTokens[email].RefreshToken + "\"}";
                var contentData = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(_apiSettings.RefreshAddress, contentData);
                string jsonstringJWT = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        myTokens[email] = JsonConvert.DeserializeObject<JwtToken>(jsonstringJWT);
                        _logger.LogInformation("RefreshToken(" + email + ") success got:" + jsonstringJWT);
                        break;
                    default:
                        _logger.LogInformation("RefreshToken("+email+") Unexpected StatusCode:"+response.StatusCode.ToString()+" got:"+ jsonstringJWT+" payload was:"+ stringData);
                        myTokens[email] = await RequestToken(email);
                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken(" + email + ")");
                fals.AccessToken = "RefreshToken Exception";
                myTokens[email] = fals;
            }
            return myTokens[email];
        }

        private async Task<JwtToken> RequestToken(string email)
        {
            JwtToken fals = new JwtToken();
            try
            {
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = 
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
                HttpClient client = new HttpClient(handler);
                client.BaseAddress = new Uri(_apiSettings.BaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string stringData = "{\"email\":\"" + email + "\",\"password\":\"" + email + "\"}";
                var contentData = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(_apiSettings.LoginAddress, contentData);
                string jsonstringJWT = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        myTokens[email] = JsonConvert.DeserializeObject<JwtToken>(jsonstringJWT);
                        break;
                    case System.Net.HttpStatusCode.BadRequest:
                        _logger.LogWarning("RequestToken BadRequest");
                        fals.AccessToken = "RequestToken BadRequest";
                        myTokens[email] = fals;
                        break;
                    default:
                        try
                        {
                            _logger.LogInformation("RequestToken Unexpected StatusCode:" + response.StatusCode.ToString());
                            _logger.LogInformation("Token:" + jsonstringJWT);
                            myTokens[email] = JsonConvert.DeserializeObject<JwtToken>(jsonstringJWT);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "RequestToken Unexpected StatusCode no JWT");
                            fals.AccessToken = "RequestToken UnexpectedStatusCode";
                            myTokens[email] = fals;
                        }
                        break;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RequestToken(" + email + ")");
                fals.AccessToken = "Exception";
                myTokens[email] = fals;
            }
            return myTokens[email];
        }
    }
}
