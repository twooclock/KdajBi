using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using KdajBi.Core.dtoModels.Requests;
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
        public ActionResult<List<TimeSlot>> Get(
            string token,
            [FromQuery] DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Today;
            }

            DateTime dateEnd = date.Value.AddDays(1).AddTicks(-1);

            AppointmentToken appointmentToken = _context.AppointmentTokens
                .Include(s => s.Company)
                .Where(s => s.Active == true)
                .FirstOrDefault(x => x.Token == token);

            if (appointmentToken == null)
            {
                return NotFound();
            }

            var workplace = _context
                .Workplaces
                .Include(s => s.WorkplaceSchedules)
                .ThenInclude(x => x.Schedule)
                .Include(e => e.WorkplaceScheduleExceptions)
                .FirstOrDefault(x => x.Id == appointmentToken.WorkplaceId);

            List<TimeSlot> workhours = getWorkplaceWorkhours(workplace, date.Value, dateEnd);

            if (workhours.Count == 0)
            {
                return Ok(new List<TimeSlot>());
            }

            List<TimeSlot> availableAppointments = TimeSlotManager.generateTimeSlots(workhours, appointmentToken.Minutes);

            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                var events = myGoogleHelper.GetAllEvents(workplace.GoogleCalendarID, date.Value, dateEnd);
                if (events == null)
                {
                    return Ok(availableAppointments);
                }
                foreach (var evt in events)
                {
                    availableAppointments = TimeSlotManager.removeOccupiedAppointments(
                        availableAppointments,
                        new TimeSlot(
                            evt.Start.DateTime.Value,
                            evt.End.DateTime.Value
                        )
                    );
                }
            }
            return Ok(availableAppointments);
        }

        private List<TimeSlot> getWorkplaceWorkhours(Workplace workplace, DateTime date, DateTime dateEnd)
        {
            List<TimeSlot> schedule = new List<TimeSlot>();

            var dayOfTheWeek = date.DayOfWeek;

            dynamic data = JObject.Parse("{}");

            var workplaceScheduleException = _context.WorkplaceScheduleExceptions.Where(x => x.WorkplaceId == workplace.Id && x.Date == date).ToList();
            if (workplaceScheduleException.Count > 0)
            {
                //extra urnik
                JArray jsonArray = JArray.Parse(workplaceScheduleException[0].EventsJson);
                foreach (var exSch in jsonArray.Children())
                {
                    data = JObject.Parse(exSch.ToString());
                    schedule.Add(new TimeSlot(
                            (DateTime)data.start.ToLocalTime(),
                            (DateTime)data.end.ToLocalTime()
                        )
                    );
                }
                return schedule;
            }

            if (workplace.Schedules.Count > 0)
            {
                //urnik za dan
                JArray jsonArray = JArray.Parse(workplace.Schedules.First().EventsJson);
                foreach (var exSch in jsonArray.Children())
                {
                    data = JObject.Parse(exSch.ToString());
                    if (data.resourceId != (int)dayOfTheWeek)
                    {
                        continue;
                    }
                    schedule.Add(
                        new TimeSlot(
                            date.Add(TimeSpan.Parse(data.startTime)),
                            date.Add(TimeSpan.Parse(data.endTime))
                        )
                    );
                }
                return schedule;
            }

            //urnik lokacije
            var location = _context.Locations.Include(s => s.Schedule).Where(x => x.Id == workplace.LocationId).FirstOrDefault();

            if (location.Schedule.EventsJson != null)
            {
                //not yet
                JArray jsonArray = JArray.Parse(location.Schedule.EventsJson);
                foreach (var exSch in jsonArray.Children())
                {
                    data = JObject.Parse(exSch.ToString());
                    data.workplaceid = workplace.Id;
                    if (data.resourceId == (int)dayOfTheWeek)
                    {
                        schedule.Add(data);
                    }
                }
                return schedule;
            }

            data.workplaceid = workplace.Id;
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
            schedule.Add(new TimeSlot(date + ((DateTime)data.start).TimeOfDay, date + ((DateTime)data.end).TimeOfDay));

            return schedule;
        }

        [AllowAnonymous]
        [HttpPost("/api/booking/{token}")]
        public ActionResult<List<TimeSlot>> Store(
            string token,
            BookingRequest bookingRequest)
        {

            AppointmentToken appointmentToken = _context.AppointmentTokens
                .Include(s => s.Company)
                .Include(s => s.Client)
                .Include(s => s.Workplace)
                .Where(x => x.Active == true)
                .FirstOrDefault(x => x.Token == token);

            // validate timeslot
            
            BookingConfirmation bookingConfirmation = new BookingConfirmation();
            bookingConfirmation.Active = true;
            bookingConfirmation.CreatedDate = DateTime.Now;
            bookingConfirmation.AppointmentToken = appointmentToken;
            bookingConfirmation.Start = bookingRequest.TimeSlot.start;
            bookingConfirmation.End = bookingRequest.TimeSlot.end;

            
            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                var gEvent = myGoogleHelper.AddEvent(
                    appointmentToken.Workplace.GoogleCalendarID,
                    "Čaka na potrditev: " + appointmentToken.Client.FullName + " - " + appointmentToken.Service,
                    null,
                    null,
                    bookingRequest.TimeSlot.start,
                    bookingRequest.TimeSlot.end
                );
                
                bookingConfirmation.GCalId = gEvent.Id;
            }
            _context.BookingConfirmations.Add(bookingConfirmation);
            
            appointmentToken.Active = false;
            _context.SaveChanges();

            return Ok();
        }
    }
}
