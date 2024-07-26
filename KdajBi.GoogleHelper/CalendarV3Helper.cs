using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Http;
using Google.Apis.Requests;
using Google.Apis.Services;
using Newtonsoft.Json;
using NLog;
using static Google.Apis.Calendar.v3.Data.Event;
using static Google.Apis.Calendar.v3.EventsResource.ListRequest;

namespace KdajBi.GoogleHelper
{
    public class CalendarV3Helper : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static string[] scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "My project";

        CalendarService myCALservice;

        //used to communicate with google via service_account (json file)
        public CalendarV3Helper(string p_jsonFileName)
        {
            myCALservice = GetService(p_jsonFileName);
        }
        private CalendarService GetService(string p_jsonFileName)
        {
            try
            {
                GoogleCredential credential;

                using (var stream = new FileStream(p_jsonFileName, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
                }
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                return service;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }


        public IEnumerable<Event> GetAllEvents(string calendarId, DateTime? startData, DateTime? endData = null)
        {
            if (calendarId == null)
            {
                return new List<Event>();
            }
            try
            {
                EventsResource.ListRequest request = myCALservice.Events.List(calendarId);
                if (startData != null) { request.TimeMinDateTimeOffset = startData.Value; }
                if (endData != null) { request.TimeMaxDateTimeOffset = endData.Value; }

                request.ShowDeleted = false;
                request.OrderBy =OrderByEnum.StartTime;
                request.SingleEvents = true;
                request.MaxResults = 2500;
                Events events;
                List<Event> retval = new List<Event>();
                string nextpageToken = null;
                do
                {
                    if (nextpageToken != null) { request.PageToken = nextpageToken; }
                    events = request.Execute();
                    retval.AddRange(events.Items);
                    nextpageToken = events.NextPageToken;
                } while (nextpageToken != null);
                return retval;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "calendarid:" + calendarId);
                return null;
            }
        }


        public Event AddEvent(string calendarId, string title, DateTime startTime, DateTime endTime, long p_ClientId, string p_ClientFullName, string p_ClientMobile, string p_Notes)
        {
            Event.ExtendedPropertiesData myExtendedProperties = new Event.ExtendedPropertiesData();
            myExtendedProperties.Shared = new Dictionary<string, string>();
            if (p_ClientId > 0)
            {
                string objClient = JsonConvert.SerializeObject(new
                {
                    label = p_ClientFullName,
                    value = p_ClientId,
                    mobile = p_ClientMobile
                });
                myExtendedProperties.Shared.Add(new KeyValuePair<string, string>("clientid", p_ClientId.ToString()));
                myExtendedProperties.Shared.Add(new KeyValuePair<string, string>("client", objClient));
            }
            myExtendedProperties.Shared.Add(new KeyValuePair<string, string>("notes", p_Notes));

            Event newEvent = new Event()
            {
                Summary = title,
                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = startTime
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = endTime
                },
                ExtendedProperties = myExtendedProperties

            };
            
            
            return myCALservice.Events.Insert(newEvent, calendarId).Execute();
        }

        public Event UpdateEvent(string title, string calendarId, string eventId)
        {
            EventsResource.GetRequest getRequest = myCALservice.Events.Get(calendarId, eventId);
            //Console.WriteLine(calendarId);
            //Console.WriteLine(eventId);
            Event evt = getRequest.Execute();
            
            evt.Summary = title;

            return myCALservice.Events.Update(evt, calendarId, eventId).Execute();
        }

        public void DeleteEvent( string calendarId, string eventId)
        {
            try
            {
                myCALservice.Events.Delete(calendarId, eventId).Execute();
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.Gone) { return; } //ignore if already deleted
                throw;
            }
        }

        public void Dispose()
        {
            if (myCALservice != null) { myCALservice.Dispose(); }
        }
    }
}
