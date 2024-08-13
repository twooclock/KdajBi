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
using System.Globalization;

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
        /// <summary>
        /// returns a list of available timeslots for a specified token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="date"></param>
        /// <returns></returns>
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

            List<TimeSlot> availableAppointments = TimeSlotManager.generateTimeSlots(workhours, appointmentToken.Minutes, 0);

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
                            evt.Start.DateTimeDateTimeOffset.Value.LocalDateTime,
                            evt.End.DateTimeDateTimeOffset.Value.LocalDateTime
                        )
                    );
                }
            }
            return Ok(availableAppointments);
        }

        private List<TimeSlot> getWorkplaceWorkhours(Workplace p_workplace, DateTime date, DateTime dateEnd)
        {
            List<TimeSlot> schedule = new List<TimeSlot>();

            var dayOfTheWeek = date.DayOfWeek;

            dynamic data = JObject.Parse("{}");

            var workplaceScheduleException = _context.WorkplaceScheduleExceptions.Where(x => x.WorkplaceId == p_workplace.Id && x.Date == date).ToList();
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

            if (p_workplace.Schedules.Count > 0)
            {
                //urnik za dan
                //check setting cbEmployee_AlternatingWeeks to determine appropriate schedule
                long companyId = _context.Locations.Where(l => l.Id == p_workplace.LocationId).FirstOrDefault().CompanyId;
                long schTypeId = 0;
                if (bool.Parse(SettingsHelper.getSetting(_context, companyId, null, "cbEmployee_AlternatingWeeks", "false")) == true)
                {
                    //synchronized with FullCalendar.cs function getRRule
                    int weeks = (int)Math.Ceiling((((date - new DateTime(2018, 01, 01)).TotalDays + 1) / 7));
                    //prej
                    //Calendar cal = new CultureInfo("en-US").Calendar; //TODO!
                    //int weeks = cal.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    //var wno = ISOWeek.GetWeekOfYear(date);
                    if (weeks % 2 == 0) { schTypeId = 2; } else { schTypeId = 1; }
                    if (p_workplace.Schedules.Where(s => s.Type == schTypeId).FirstOrDefault() == null)
                    { schTypeId = 0; }
                }
                
                JArray jsonArray = JArray.Parse(p_workplace.Schedules.Where(s => s.Type == schTypeId).First().EventsJson);
                foreach (var exSch in jsonArray.Children())
                {
                    data = JObject.Parse(exSch.ToString());
                    if (data.resourceId != (int)dayOfTheWeek)
                    {
                        continue;
                    }
                    schedule.Add(
                        new TimeSlot(
                            date.Add(TimeSpan.Parse(data.startTime.ToString())),
                            date.Add(TimeSpan.Parse(data.endTime.ToString()))
                        )
                    );
                }
                return schedule;
            }

            //urnik lokacije
            var location = _context.Locations.Include(s => s.Schedule).Where(x => x.Id == p_workplace.LocationId).FirstOrDefault();
            
            if (location.Schedule.EventsJson != null)
            {
                //not yet
                JArray jsonArray = JArray.Parse(location.Schedule.EventsJson);
                foreach (var exSch in jsonArray.Children())
                {
                    data = JObject.Parse(exSch.ToString());
                    data.workplaceid = p_workplace.Id;
                    if (data.resourceId == (int)dayOfTheWeek)
                    {
                        schedule.Add(data);
                    }
                }
                return schedule;
            }

            data.workplaceid = p_workplace.Id;
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

        /// <summary>
        /// makes an appointment (adds an event to google calendar)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="bookingRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/booking/{token}")]
        public ActionResult<List<TimeSlot>> Store(
            string token,
            BookingRequest bookingRequest)
        {

            AppointmentToken appointmentToken = _context.AppointmentTokens
                .Include(s => s.Location)
                .Include(s => s.Client)
                .Include(s => s.Workplace)
                .Where(x => x.Active == true)
                .FirstOrDefault(x => x.Token == token);

            appointmentToken.BookingCreated = DateTime.Now;
            appointmentToken.Start = bookingRequest.TimeSlot.start;
            appointmentToken.End = bookingRequest.TimeSlot.end;
            
            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                var gEvent = myGoogleHelper.AddEvent(
                    appointmentToken.Workplace.GoogleCalendarID,
                    "Čaka na potrditev: " + appointmentToken.Client.FullName +  " ("+ appointmentToken.Client.Mobile + ") - " + appointmentToken.Service,
                    bookingRequest.TimeSlot.start, bookingRequest.TimeSlot.end,
                    appointmentToken.Client.Id, appointmentToken.Client.FullName, 
                    appointmentToken.Client.Mobile, appointmentToken.Service
                );

                appointmentToken.GCalId = gEvent.Id;
            }
            
            appointmentToken.Active = false;

            if (bool.Parse(SettingsHelper.getSetting(_context, appointmentToken.Location.CompanyId, appointmentToken.Location.Id, "PublicBooking_AlertMeWithSMS", "true")) == true)
            {
                //alert about new appointment
                //(TODO: naredi prek service)
                SmsCampaign newSmsCampaign = new SmsCampaign();
                newSmsCampaign.Company.Id = appointmentToken.Location.CompanyId;
                newSmsCampaign.LocationId = appointmentToken.Location.Id;
                newSmsCampaign.AppointmentTokenId = appointmentToken.Id;
                var myUser = _context.Users.Where(c => c.CompanyId == appointmentToken.Location.CompanyId).OrderBy(o => o.Id).AsNoTracking().First();
                newSmsCampaign.AppUser.Id = myUser.Id;

                newSmsCampaign.MsgTxt = @"Novo narocilo prek spleta! Poglej v https://KdajBi.si";
                var mySmsInfo = new SmsCounter(newSmsCampaign.MsgTxt);

                newSmsCampaign.MsgSegments = mySmsInfo.Messages;
                newSmsCampaign.Name = "PublicBookingAlert";
                newSmsCampaign.Recipients.Add(new SmsMsg(appointmentToken.Location.Tel.Replace(" ", ""), 0));

                newSmsCampaign.SendAfter = DateTime.Now;
                newSmsCampaign.ApprovedAt = DateTime.Now;

                _context.Attach(newSmsCampaign.Company);
                _context.Attach(newSmsCampaign.AppUser);
                _context.SmsCampaigns.Add(newSmsCampaign);
            }
            _context.SaveChanges();


            return Ok();
        }
    }
}
