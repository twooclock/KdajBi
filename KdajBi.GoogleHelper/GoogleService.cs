using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            GoogleAuthToken myToken = JsonConvert.DeserializeObject<GoogleAuthToken>(p_Token);
            myToken.refresh_token = "";
            return JsonConvert.SerializeObject(myToken);
        }
    }

    public class GoogleService : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public readonly GoogleAuthToken googleAuthToken = new GoogleAuthToken();
        private CalendarService calendarService = new CalendarService();
        

        public bool TokenWasRefreshed = false;
        

        public GoogleService(string p_userid, GoogleAuthToken p_googleAuthToken)
        {
            googleAuthToken = p_googleAuthToken;
            if (p_googleAuthToken.expires_at.AddMinutes(-5) < DateTime.UtcNow)
            {
                TokenWasRefreshed = true;
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var tokenResponse = new TokenResponse()
                {
                    AccessToken = p_googleAuthToken.access_token,
                    RefreshToken = p_googleAuthToken.refresh_token
                };
                ClientSecrets cSecrets = new ClientSecrets() { ClientId = config["GoogleSettings:ClientId"], ClientSecret = config["GoogleSettings:ClientSecret"] };

                var flowInitializer = new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = cSecrets };

                var flow = new GoogleAuthorizationCodeFlow(flowInitializer);

                var credential = new UserCredential(flow, p_userid, tokenResponse);
                var dobil = credential.RefreshTokenAsync(CancellationToken.None).GetAwaiter().GetResult();

                if (credential.Token.RefreshToken != null) { Logger.Info("GOT New token.RefreshToken:{0}", credential.Token.RefreshToken.ToString()); }

                googleAuthToken.access_token = credential.Token.AccessToken;
                if (credential.Token.RefreshToken != null)
                {
                    googleAuthToken.refresh_token = credential.Token.RefreshToken;
                }
                googleAuthToken.expires_at = credential.Token.IssuedUtc.AddSeconds((double)credential.Token.ExpiresInSeconds);
            }
        }

        public void Dispose()
        {
            calendarService.Dispose();
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
                    request.MinAccessRole = CalendarListResource.ListRequest.MinAccessRoleEnum.Writer;
                    retval = request.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Can't getCalendars()");
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
                request.TimeMin = firstDayOfMonth;
                request.TimeMax = lastDayOfMonth;
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
