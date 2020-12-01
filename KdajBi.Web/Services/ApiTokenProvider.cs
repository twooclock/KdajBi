﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
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
        JwtToken GetToken(string email);
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
        private Dictionary<string, JwtToken> myToken;
        public ApiTokenProvider(IOptions<ApiSettings> apiSettings)
        {
            _apiSettings = apiSettings.Value;
            myToken = new Dictionary<string, JwtToken>();
        }


        public JwtToken GetToken(string email)
        {
            if (myToken.ContainsKey(email) == true)
            {
                if (myToken[email].Expiration == 0)
                {
                    //request token
                    myToken[email] = RequestToken(email);
                }
                else
                {
                    if (new DateTime(myToken[email].Expiration, DateTimeKind.Utc) <= DateTime.UtcNow)
                    {
                        //refresh token if expired
                        myToken[email] = RefreshToken(email);

                    }

                }
            }
            else { myToken[email] = RequestToken(email); }

            return myToken[email];
        }

        public ApiSettings Settings()
        {
            return _apiSettings;
        }

        private JwtToken RefreshToken(string email)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_apiSettings.BaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + myToken[email].AccessToken);
                string stringData = "{\"UserEmail\":\"" + _apiSettings.UserName + "\",\"Token\":\"" + myToken[email].RefreshToken + "\"}";
                var contentData = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(_apiSettings.RefreshAddress, contentData).Result;
                string jsonstringJWT = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                { myToken[email] = JsonConvert.DeserializeObject<JwtToken>(jsonstringJWT); }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    if (jsonstringJWT.StartsWith("\"Expired"))
                    { myToken[email] = RequestToken(email); }
                    else
                    { myToken[email] = new JwtToken(); }
                }

            }
            catch (Exception ex)
            {
                myToken[email] = new JwtToken();
            }
            return myToken[email];
        }

        private JwtToken RequestToken(string email)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_apiSettings.BaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string stringData = "{\"email\":\"" + email + "\",\"password\":\"" + email + "\"}";
                var contentData = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(_apiSettings.LoginAddress, contentData).Result;
                string jsonstringJWT = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                { myToken[email] = JsonConvert.DeserializeObject<JwtToken>(jsonstringJWT); }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    myToken[email] = new JwtToken();
                }
            }
            catch (Exception ex)
            {
                myToken[email] = new JwtToken();
            }
            return myToken[email];
        }
    }
}