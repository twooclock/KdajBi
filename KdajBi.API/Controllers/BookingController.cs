using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [ApiController]
    public class BookingController : _BaseController
    {
        protected readonly ICalendarV3Provider _calendarV3Provider;

        public BookingController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<AppointmentTokenController> logger,
            IEmailSender emailSender,
            ICalendarV3Provider calendarV3Provider
            )
            : base(context, userManager, signInManager, logger, emailSender)
        {
            _calendarV3Provider = calendarV3Provider;
        }

        [AllowAnonymous]
        [HttpGet("/api/booking/{token}")]
        public ActionResult<AppointmentToken> Get(
            string token,
            [FromQuery] DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Today;
            }

            AppointmentToken appointmentToken = _context.AppointmentTokens.Include(s => s.Company).FirstOrDefault(x => x.Token == token);

            //We need to generate list of available time slots here
            //upoštevam urnik
            //daj proste termine od-do
            //zaenkrat za en dan
            var workplaces = _context.Workplaces.Include(s => s.WorkplaceSchedules).Where(x => x.LocationId == appointmentToken.LocationId);
            //add google clendar names
            //var gt = _CurrentUserGooToken();
            //if (gt != null)
            //{
            //    using (GoogleService service = new GoogleService(gt))
            //    {
            //        foreach (var gooCalendar in service.getCalendars().Items)
            //        {
            //            foreach (var item in data)
            //            {
            //                if (gooCalendar.Id == item.GoogleCalendarID)
            //                { item.GoogleCalendarSummary = gooCalendar.Summary; }
            //            }
            //        }
            //    }
            //}

            //TODO: manjka kateri stilist (workplace)?
            // String[,] appointmets = new String[,] {};
            String[,] appointments = new String[,] {
                {"10:00", "14:00"},
                {"14:00", "18:00"},
                {"15:00", "19:00"},
                {"16:00", "20:00"}
            };
            return Ok(appointments);
        }

        [AllowAnonymous]
        [HttpGet("/api/booking/")]
        public ActionResult GetEmptyTimeSlots(
            [FromQuery] DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Today;
            }
            DateTime dateEnd = date.Value.AddDays(1).AddTicks(-1);
            var dayOfTheWeek = date.Value.DayOfWeek;
            //We need to generate list of available time slots here
            //upoštevam urnik
            //daj proste termine od-do
            //zaenkrat za en dan
            List<KeyValuePair<long, List<TimeSlot>>> availableTimeSlots = new List<KeyValuePair<long, List<TimeSlot>>>();
            var workplaces = _context.Workplaces.Include(s => s.WorkplaceSchedules).ThenInclude(x => x.Schedule).Include(e => e.WorkplaceScheduleExceptions).Where(x => x.LocationId == 6).ToList();

            foreach (var item in workplaces)
            {
                List<TimeSlot> urniki = new List<TimeSlot>();
                var workplaceScheduleException = _context.WorkplaceScheduleExceptions.Where(x => x.WorkplaceId == item.Id && x.Date == date).ToList();
                if (workplaceScheduleException.Count > 0)
                {
                    //extra urnik
                    JArray jsonArray = JArray.Parse(workplaceScheduleException[0].EventsJson);
                    foreach (var exSch in jsonArray.Children())
                    {
                        dynamic data = JObject.Parse(exSch.ToString());
                        urniki.Add(new TimeSlot((DateTime)data.start.Value.ToLocalTime(), (DateTime)data.end.Value.ToLocalTime()));
                    }
                }
                else
                {
                    if (item.Schedules.Count > 0)
                    {
                        //urnik za dan
                        JArray jsonArray = JArray.Parse(item.Schedules.First().EventsJson);
                        foreach (var exSch in jsonArray.Children())
                        {
                            dynamic data = JObject.Parse(exSch.ToString());
                            if (data.resourceId == (int)dayOfTheWeek) { urniki.Add(new TimeSlot(date.Value.Add(TimeSpan.Parse(data.startTime.Value)), date.Value.Add(TimeSpan.Parse(data.endTime.Value)))); }
                        }
                    }
                    else
                    {
                        //urnik lokacije
                        var location = _context.Locations.Include(s => s.Schedule).Where(x => x.Id == 6).FirstOrDefault();
                        if (location.Schedule.EventsJson != null)
                        {
                            //not yet
                            JArray jsonArray = JArray.Parse(location.Schedule.EventsJson);
                            foreach (var exSch in jsonArray.Children())
                            {
                                dynamic data = JObject.Parse(exSch.ToString());
                                data.workplaceid = item.Id;
                                if (data.resourceId == (int)dayOfTheWeek) { urniki.Add(data); }
                            }
                        }
                        else
                        {
                            dynamic data = JObject.Parse("{}");
                            data.workplaceid = item.Id;
                            switch (dayOfTheWeek)
                            {
                                case DayOfWeek.Sunday:
                                    data.start = location.Schedule.SundayStart;
                                    data.end = location.Schedule.SundayEnd;
                                    break;
                                case DayOfWeek.Monday:
                                    data.start = location.Schedule.MondayStart;
                                    data.end = location.Schedule.MondayEnd;
                                    break;
                                case DayOfWeek.Tuesday:
                                    data.start = location.Schedule.TuesdayStart;
                                    data.end = location.Schedule.TuesdayEnd;
                                    break;
                                case DayOfWeek.Wednesday:
                                    data.start = location.Schedule.WednesdayStart;
                                    data.end = location.Schedule.WednesdayEnd;
                                    break;
                                case DayOfWeek.Thursday:
                                    data.start = location.Schedule.ThursdayStart;
                                    data.end = location.Schedule.ThursdayEnd;
                                    break;
                                case DayOfWeek.Friday:
                                    data.start = location.Schedule.FridayStart;
                                    data.end = location.Schedule.FridayEnd;
                                    break;
                                case DayOfWeek.Saturday:
                                    data.start = location.Schedule.SaturdayStart;
                                    data.end = location.Schedule.SaturdayEnd;
                                    break;
                                default:
                                    break;
                            }
                            urniki.Add(new TimeSlot(date.Value+((DateTime)data.start).TimeOfDay, date.Value + ((DateTime)data.end).TimeOfDay));
                        }
                    }
                }
                if (urniki.Count>0)
                { 
                //pridobi naročila in zračunaj preostale proste termine
                using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
                {
                    var events = myGoogleHelper.GetAllEvents(item.GoogleCalendarID, date, dateEnd);
                    if (events != null)
                    {
                        foreach (var evt in events)
                        {
                            Console.WriteLine(evt.Summary);
                            urniki = TimeSlotManager.AddEvent(urniki, new TimeSlot(evt.Start.DateTime.Value, evt.End.DateTime.Value));
                        }
                    }
                }
                }
                //prosti termini zračunani
                availableTimeSlots.Add(new KeyValuePair<long, List<TimeSlot>>(item.Id, urniki));
            }


            //TODO: manjka kateri stilist (workplace)?
            
            
            return Ok(availableTimeSlots);
        }
    }
}
