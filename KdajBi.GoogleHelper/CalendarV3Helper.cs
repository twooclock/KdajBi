using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KdajBi.GoogleHelper
{
    public class CalendarV3Helper : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static string[] scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "My project";

        CalendarService myCALservice = null;

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
            try
            {
                EventsResource.ListRequest request = myCALservice.Events.List(calendarId);
                if (startData != null) { request.TimeMin = startData.Value; }
                if (endData != null) { request.TimeMax = endData.Value; }

                request.ShowDeleted = false;
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


        public void AddEvent(string calendarId, string title, string contents, string location, DateTime startTime, DateTime endTime)
        {

            Event newEvent = new Event()
            {
                Summary = title,
                Location = location,
                Description = contents,
                Start = new EventDateTime()
                {
                    DateTime = startTime
                },
                End = new EventDateTime()
                {
                    DateTime = endTime
                }

            };

            EventsResource.InsertRequest insRequest = myCALservice.Events.Insert(newEvent, calendarId);
            Event createdEvent = insRequest.Execute();
        }

        public void Dispose()
        {
            if (myCALservice != null) { myCALservice.Dispose(); }
        }
    }
}
