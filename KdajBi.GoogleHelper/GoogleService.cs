using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace KdajBi.GoogleHelper
{

    public class GoogleAuthToken
    {
        public string access_token;
        public string refresh_token;
        public DateTime expires_at;
        public string token_type;
        public GoogleAuthToken() { }
    }

    public static class TokenHandler
    {
        public static string RemoveRefreshToken(string p_Token)
        {
            GoogleAuthToken googleAuthToken = JsonConvert.DeserializeObject<GoogleAuthToken>(p_Token);
            googleAuthToken.refresh_token = "";
            return JsonConvert.SerializeObject(googleAuthToken);
        }
    }

    //used to communicate with google via GoogleAuthToken (user must be authenticated via Google)
    public class GoogleService : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public readonly GoogleAuthToken googleAuthToken = new GoogleAuthToken();
        private CalendarService calendarService = new CalendarService();


        public bool TokenWasRefreshed = false;


        public GoogleService(string p_userid, GoogleAuthToken p_googleAuthToken)
        {
            googleAuthToken = p_googleAuthToken;
            Logger.Info("New GoogleService for " + p_userid + " with token that expires on (UTC) " + googleAuthToken.expires_at.ToString());
            if (p_googleAuthToken.expires_at.AddMinutes(-5) < DateTime.UtcNow)
            {
                Logger.Info("GoogleService have to refresh token for " + p_userid + " since "+ p_googleAuthToken.expires_at.AddMinutes(-5).ToString()+" < "+ DateTime.UtcNow.ToString() + " / access token expires on (UTC) " + googleAuthToken.expires_at.ToString());
                try
                {
                    TokenWasRefreshed = true;
                    if (p_googleAuthToken.refresh_token.Length == 0)
                    {
                        TokenWasRefreshed = false;
                        Logger.Warn("GoogleService should RefreshAccessToken but RefreshToken is missing for "+ p_userid + "!"); }
                    else
                    { var rt = RefreshTokenAsync(p_userid, p_googleAuthToken).Result; }
                    //googleAuthToken.access_token = rt.access_token;
                    Logger.Info("GoogleService TokenWasRefreshed=" + TokenWasRefreshed.ToString() + " for "+ p_userid + " / access token expires on (UTC) " + googleAuthToken.expires_at.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex,"Error refreshing token");
                    throw;
                }
            }
            else
            { Logger.Info("GoogleService skipped token refresh with token that expires on (UTC) " + googleAuthToken.expires_at.ToString()); }
        }

        private async Task<GoogleAuthToken> RefreshTokenAsync(string p_userid, GoogleAuthToken p_googleAuthToken)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var tokenResponse = new TokenResponse()
            {
                AccessToken = p_googleAuthToken.access_token,
                RefreshToken = p_googleAuthToken.refresh_token
            };
            ClientSecrets cSecrets = new ClientSecrets() { ClientId = config["GoogleSettings:ClientId"], ClientSecret = config["GoogleSettings:ClientSecret"] };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = cSecrets });

            var credential = new UserCredential(flow, p_userid, tokenResponse);

            var dobil = await credential.RefreshTokenAsync(CancellationToken.None);
            TokenWasRefreshed = dobil;
            Logger.Info("credential.RefreshTokenAsync returned:{0}", dobil.ToString());
            if (googleAuthToken.access_token != credential.Token.AccessToken)
            {
                Logger.Info("GOT New token.AccessToken:{0}", credential.Token.AccessToken.ToString());
            }
            else { Logger.Info("FAILED TO GET New token.AccessToken with refreshToken:{0}", p_googleAuthToken.refresh_token); }
            googleAuthToken.access_token = credential.Token.AccessToken;
            if (credential.Token.RefreshToken != null)
            {
                googleAuthToken.refresh_token = credential.Token.RefreshToken;
                Logger.Info("GOT New token.RefreshToken:{0}", credential.Token.RefreshToken.ToString());
            }
            if (credential.Token.ExpiresInSeconds.HasValue)
            { googleAuthToken.expires_at = credential.Token.IssuedUtc.AddSeconds((double)credential.Token.ExpiresInSeconds); }
            
            return googleAuthToken;
        }

        public void Dispose()
        {
            calendarService.Dispose();
        }

        public async Task<string> CreateCalendar(string p_Summary)
        {
            try
            {
                if (googleAuthToken != null)
                {
                    Google.Apis.Calendar.v3.Data.Calendar retval = null;
                    Google.Apis.Calendar.v3.Data.Calendar newCalendar = new Google.Apis.Calendar.v3.Data.Calendar
                    {
                        Summary = p_Summary
                    };
                    var request = calendarService.Calendars.Insert(newCalendar);
                    request.OauthToken = googleAuthToken.access_token;
                    retval = await request.ExecuteAsync();
                    return retval.Id;
                }
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Couldn't create calendar " + p_Summary);
            }
            return null;
        }

        public CalendarList getCalendars()
        {
            CalendarList retval = new CalendarList();
            if (googleAuthToken != null)
            {
                try
                {
                    var request = calendarService.CalendarList.List();
                    request.OauthToken = googleAuthToken.access_token;
                    request.MinAccessRole = CalendarListResource.ListRequest.MinAccessRoleEnum.Reader;
                    retval = request.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Can't getCalendars() with token that expires on " + googleAuthToken.expires_at.ToString());
                }
            }
            return retval;
        }

        /// <summary>
        /// returns list of events FOR A MONTH!
        /// </summary>
        /// <param name="p_calendarID"></param>
        /// <param name="p_Date"></param>
        /// <returns></returns>
        public List<Event> GetEvents(string p_calendarID, DateTime p_Date)
        {
            var firstDayOfMonth = new DateTime(p_Date.Year, p_Date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);

            EventsResource.ListRequest request = calendarService.Events.List(p_calendarID);
            Events events;
            List<Event> retval = new List<Event>();
            string nextpageToken = null;
            do
            {
                request.OauthToken = googleAuthToken.access_token;
                if (nextpageToken != null) { request.PageToken = nextpageToken; }
                request.TimeMinDateTimeOffset = firstDayOfMonth;
                request.TimeMaxDateTimeOffset = lastDayOfMonth;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                //request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                events = request.Execute();
                retval.AddRange(events.Items);
                nextpageToken = events.NextPageToken;
            } while (nextpageToken != null);


            return retval;
        }

        public bool EnsureReadPermissionsForService(string p_GooCalsJson, string p_GooServiceName)
        {
            bool retval = true;
            List<AclRule> myacl = new List<AclRule>();
            var myCals = JsonConvert.DeserializeObject<string[]>(p_GooCalsJson);
            foreach (var item in myCals)
            {
                var request = calendarService.Acl.List(item);
                request.OauthToken = googleAuthToken.access_token;
                try
                {
                    var myaclx = request.Execute();
                    myacl = (List<AclRule>)myaclx.Items;
                }
                catch (Exception ex)
                {
                    retval = false;
                    Logger.Error(ex, "Can't Acl.List(" + item + ")");
                }
                bool serviceFound = false;
                foreach (var myaclrule in myacl)
                {
                    if (myaclrule.Scope.Type == "user" && myaclrule.Scope.Value == p_GooServiceName)
                    { serviceFound = true; }
                }
                if (serviceFound == false)
                {
                    //add p_GooServiceName as writer(?) TODO:reader
                    AclRule newRule = new AclRule();
                    newRule.Role = "writer";
                    AclRule.ScopeData newScope = new AclRule.ScopeData();
                    newScope.Type = "user";
                    newScope.Value = p_GooServiceName;
                    newRule.Scope = newScope;
                    var request2 = calendarService.Acl.Insert(newRule, item);
                    request2.OauthToken = googleAuthToken.access_token;
                    try
                    {
                        var mynewacl = request2.Execute();
                    }
                    catch (Exception ex)
                    {
                        retval = false;
                        Logger.Error(ex, "Can't Acl.Insert(" + item + ")");
                    }

                }
            }
            return retval;
        }

    }
}
