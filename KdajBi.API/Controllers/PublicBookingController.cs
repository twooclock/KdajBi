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
using System.ComponentModel.Design;
using Z.Expressions;
using Google.Apis.Util;

namespace KdajBi.API.Controllers
{
    [ApiController]
    public class PublicBookingController : _BaseController
    {
        protected readonly ICalendarV3Provider _calendarV3Provider;

        public PublicBookingController(
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

        private async Task<List<TimeSlot>> GetFreeTimeSlots(long lid, long wpid, long sid, DateTime date, int move)
        {
            List<TimeSlot> allUnqTimeSlots = new List<TimeSlot>();
            try { 
                DateTime dateEnd = date.AddDays(1).AddTicks(-1);

                var myService = await _context.Services.FindAsync(sid);
                if (myService == null) { return new List<TimeSlot>(); }

                List<BookingTimeslots> retval = new List<BookingTimeslots>();

                List<Workplace> myWP = new List<Workplace>();
                if (wpid > 0)
                {
                    myWP.Add(await _context
                    .Workplaces
                    .Include(s => s.WorkplaceSchedules)
                    .ThenInclude(x => x.Schedule)
                    .Include(e => e.WorkplaceScheduleExceptions)
                    .FirstOrDefaultAsync(x => x.Id == wpid && x.LocationId == lid));
                }
                else
                {
                    myWP.AddRange(_context
                   .Workplaces
                   .Include(s => s.WorkplaceSchedules)
                   .ThenInclude(x => x.Schedule)
                   .Include(e => e.WorkplaceScheduleExceptions)
                   .Where(x => x.LocationId == lid));
                }
                //remove workplaces that do not provide service specified
                for (int i = myWP.Count - 1; i >= 0; i--)
                {
                    if ((await _context.WorkplaceExcludedServices.Where(e => e.WorkplaceId == myWP[i].Id && e.ServiceId == myService.Id).CountAsync()) > 0)
                    { myWP.RemoveAt(i); }
                }
                //remove already booked timeslots
                using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
                {
                    foreach (var wp in myWP)
                    {
                        List<TimeSlot> availableAppointments = new List<TimeSlot>();
                        List<TimeSlot> availableworkhours = new List<TimeSlot>();
                        availableworkhours = await getWorkplaceWorkhours(wp, date);
                        if (wp.SequentialBooking == true)
                        {   //omogoči le zaporedna naročila (prosti termini so le pred ali po obstoječem naročilu)
                            availableAppointments = new List<TimeSlot>();

                            foreach (var timeslot in availableworkhours)
                            {
                                //za vsako naročilo, generiraj dva prosta termina (prej in po)
                                var events = myGoogleHelper.GetAllEvents(wp.GoogleCalendarID, timeslot.start, timeslot.end);
                                if (events != null)
                                {
                                    if (events.Count()>0 ) { 
                                        foreach (var evt in events)
                                        {
                                            //dodaj prej
                                            if (evt.Start.DateTimeDateTimeOffset.Value.LocalDateTime.AddMinutes(-myService.Minutes)>= timeslot.start) {
                                                availableAppointments.Add(
                                                    new TimeSlot(wp.Id,
                                                        evt.Start.DateTimeDateTimeOffset.Value.LocalDateTime.AddMinutes(-myService.Minutes),
                                                        evt.End.DateTimeDateTimeOffset.Value.LocalDateTime.AddMinutes(-myService.Minutes)
                                                    )
                                                );
                                            }
                                            //dodaj po
                                            if (evt.End.DateTimeDateTimeOffset.Value.LocalDateTime.AddMinutes(myService.Minutes) <= timeslot.end) {
                                                availableAppointments.Add(
                                                    new TimeSlot(wp.Id,
                                                        evt.End.DateTimeDateTimeOffset.Value.LocalDateTime,
                                                        evt.End.DateTimeDateTimeOffset.Value.LocalDateTime.AddMinutes(myService.Minutes)
                                                    )
                                                );
                                            }
                                        }
                                        //odstrani tiste, ki se prekrivajo z naročili
                                        foreach (var evt in events)
                                        {
                                            availableAppointments = TimeSlotManager.removeOccupiedAppointments(
                                                availableAppointments,
                                                new TimeSlot(wp.Id,
                                                    evt.Start.DateTimeDateTimeOffset.Value.LocalDateTime,
                                                    evt.End.DateTimeDateTimeOffset.Value.LocalDateTime
                                                )
                                            );
                                        }
                                    } 
                                    else
                                    {
                                        //v tem obdobju ni še nobenega naročila -> ponudi vse timeslote
                                        var myTSlist = new List<TimeSlot>();
                                        myTSlist.Add(timeslot);
                                        availableAppointments.AddRange(TimeSlotManager.generateTimeSlots(myTSlist, myService.Minutes, myService.Offset));
                                    }

                                }
                            }
                            if (availableAppointments.Count > 0)
                            { retval.Add(new BookingTimeslots(wp.Id, availableAppointments)); }
                        }
                        else
                        { //po starem (ponudi vse termine, glede na urnik in vpisana naročila)
                            availableAppointments = new List<TimeSlot>();
                            if (availableworkhours.Count > 0)
                            {
                                availableAppointments = TimeSlotManager.generateTimeSlots(availableworkhours, myService.Minutes, myService.Offset);
                                if (availableAppointments.Count > 0)
                                {
                                    var events = myGoogleHelper.GetAllEvents(wp.GoogleCalendarID, date, dateEnd);
                                    if (events != null)
                                    {
                                        foreach (var evt in events)
                                        {
                                            availableAppointments = TimeSlotManager.removeOccupiedAppointments(
                                                availableAppointments,
                                                new TimeSlot(wp.Id,
                                                    evt.Start.DateTimeDateTimeOffset.Value.LocalDateTime,
                                                    evt.End.DateTimeDateTimeOffset.Value.LocalDateTime
                                                )
                                            );
                                        }
                                    }
                                }
                            }
                            if (availableAppointments.Count > 0)
                            { retval.Add(new BookingTimeslots(wp.Id, availableAppointments)); }
                        }
                        

                    }

                }
                //return only distinct timeslots
                //the one with most empty timeslots gets appointment
                retval.Sort((p, q) => p.TimeSlots.Count.CompareTo(q.TimeSlots.Count));

            
                for (int i = 0; i < retval.Count; i++)
                {
                    if (i == 0)
                    { allUnqTimeSlots.AddRange(retval[i].TimeSlots); }
                    else
                    {
                        var unqSlots = retval[i].TimeSlots.Where(p => allUnqTimeSlots.All(p2 => p2.start != p.start && p2.end != p.end)).ToList();
                        retval[i].TimeSlots.Clear();
                        retval[i].TimeSlots.AddRange(unqSlots);
                        allUnqTimeSlots.AddRange(unqSlots);
                    }

                }
                //sort by Start
                allUnqTimeSlots.Sort((p, q) => p.start.CompareTo(q.start));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFreeTimeSlots napaka date:"+date);
            }
            return allUnqTimeSlots;
        }

        /// <summary>
        /// Get a list of available timeSlots for a specified date
        /// For each workplace: get available schedule, divide it by service duration, get events for that day and remove busy slots
        /// Finnaly return only distinct timeslots
        /// </summary>
        /// <param name="lid">Location id</param>
        /// <param name="wpid">Workplace Id or 0 if not specified</param>
        /// <param name="sid">Service id</param>
        /// <param name="date"></param>
        /// <param name="move"></param>
        /// <param name="mindate"></param>
        /// <param name="maxdate"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/publicbooking/{lid}/{wpid}/{sid}")]
        public async Task<ActionResult<List<TimeSlot>>> Get(
            long lid, long wpid, long sid,
            [FromQuery] DateTime? date = null,
            [FromQuery] int? move = null,
            [FromQuery] DateTime? mindate = null,
            [FromQuery] DateTime? maxdate = null
            )
        {
            if (mindate == null) { _logger.LogInformation("mindate was null"); }
            if (maxdate == null) { _logger.LogInformation("maxdate was null"); }
            if (date == null) { date = DateTime.Today; }
            if (move == null) { move = 0; }
            List<TimeSlot> retval = new List<TimeSlot>();
            bool repeat = false;
            do
            {
                repeat = false;
                _logger.LogInformation("GetFreeTimeSlots(date:"+date.Value+")");
                retval = await GetFreeTimeSlots(lid, wpid, sid, date.Value, move.GetValueOrDefault());
                if (retval.Count == 0 && move != 0)
                {
                    if (move == 1 && date < maxdate) { date = date.Value.AddDays(1); repeat = true; }
                    if (move == -1 && date > mindate) { date = date.Value.AddDays(-1); repeat = true; }
                }
            }
            while (repeat == true);
            
            return Ok(retval);
        }

        /// <summary>
        /// returns workplace working schedule for a date specified in the following order:
        /// 1. p_workplace.WorkplaceScheduleException 
        /// 2. p_workplace.Schedule
        /// 3. p_workplace.Location.Schedule
        /// </summary>
        /// <param name="p_workplace"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private async Task<List<TimeSlot>> getWorkplaceWorkhours(Workplace p_workplace, DateTime date)
        {
            List<TimeSlot> schedule = new List<TimeSlot>();

            var dayOfTheWeek = date.DayOfWeek;

            dynamic data = JObject.Parse("{}");

            var workplaceScheduleException = await _context.WorkplaceScheduleExceptions.Where(x => x.WorkplaceId == p_workplace.Id && x.Date == date).ToListAsync();
            if (workplaceScheduleException.Count > 0)
            {
                //extra urnik
                JArray jsonArray = JArray.Parse(workplaceScheduleException[0].EventsJson);
                foreach (var exSch in jsonArray.Children())
                {
                    data = JObject.Parse(exSch.ToString());
                    schedule.Add(new TimeSlot(p_workplace.Id,
                            ((DateTime)(data.start)).ToLocalTime(),
                            ((DateTime)(data.end)).ToLocalTime()
                        )
                    );
                }
                return schedule;
            }

            if (p_workplace.Schedules.Count > 0)
            {
                //urnik za dan
                //check setting cbEmployee_AlternatingWeeks to determine appropriate schedule
                long companyId = (await _context.Locations.Where(l => l.Id == p_workplace.LocationId).FirstOrDefaultAsync()).CompanyId;
                long schTypeId = 0;
                if (bool.Parse(SettingsHelper.getSetting(_context, companyId, null, "cbEmployee_AlternatingWeeks", "false")) == true)
                {
                    //synchronized with FullCalendar.cs function getRRule
                    int weeks = (int)Math.Ceiling((((date - new DateTime(2018, 01, 01)).TotalDays+1) / 7));
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
                        new TimeSlot(p_workplace.Id,
                            date.Add(TimeSpan.Parse(data.startTime.ToString())),
                            date.Add(TimeSpan.Parse(data.endTime.ToString()))
                        )
                    );
                }
                return schedule;
            }

            //urnik lokacije
            var location = await _context.Locations.Include(s => s.Schedule).Where(x => x.Id == p_workplace.LocationId).FirstOrDefaultAsync();
            
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
            schedule.Add(new TimeSlot(p_workplace.Id,date + ((DateTime)data.start).TimeOfDay, date + ((DateTime)data.end).TimeOfDay));

            return schedule;
        }

        /// <summary>
        /// makes an appointment (adds an event to google calendar)
        /// </summary>
        /// <param name="pbid">PublicBooking id (contains location)</param>
        /// <param name="wpid">Workplace Id </param>
        /// <param name="sid">Service id</param>
        /// <param name="bookingRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/publicbooking/{pbid}/{wpid}/{sid}")]
        public async Task<ActionResult<List<TimeSlot>>> Store(
            long pbid, long wpid, long sid,
            BookingRequest bookingRequest)
        {
            var myPB = await _context.PublicBookings.Include(pb=>pb.Workplace)
                .Include(pb => pb.Location)
                .Where(pb=>pb.Id==pbid).FirstOrDefaultAsync();
            if (myPB == null) { return NotFound(); }
            if (string.IsNullOrEmpty(myPB.GCalId)==false) { return NotFound(); } //this PB was already used
            var myWP = _context.Workplaces.Find(wpid);
            if (myWP == null) { return NotFound(); }
            var myService = _context.Services.Find(sid);
            if (myService == null) { return NotFound(); }
            myPB.ServiceId = myService.Id;
            var myClient = await _context.Clients.Where(c=>c.CompanyId==myPB.Location.CompanyId && c.Mobile.EndsWith(myPB.Mobile.Substring(1))).FirstOrDefaultAsync();
            long ClientId = 0;
            string ClientFullName = "";
            if (myClient != null) { ClientId = myClient.Id; ClientFullName = myClient.FullName; myPB.ClientId = ClientId; }
            myPB.Start = bookingRequest.TimeSlot.start;
            myPB.End = bookingRequest.TimeSlot.end;
            myPB.ClientNotes = bookingRequest.ClientNotes;

            string appointmentTitle = ClientFullName + " (" + myPB.Mobile + ") - " + myService.Name;
            bool appointmentAutoApprove = bool.Parse(SettingsHelper.getSetting(_context, myPB.Location.CompanyId, myPB.Location.Id, "PublicBooking_AutoApprove", "false"));
            if (appointmentAutoApprove == true)
            {
                myPB.Status = 1; //confirmed
                myPB.UpdatedDate = DateTime.Now;
            }
            else
            { appointmentTitle = "Čaka na potrditev: " +appointmentTitle; }

            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                var gEvent = myGoogleHelper.AddEvent(
                    myWP.GoogleCalendarID,
                    appointmentTitle,
                    bookingRequest.TimeSlot.start, bookingRequest.TimeSlot.end,
                    ClientId, ClientFullName,
                    myPB.Mobile, myService.Name
                );
                myPB.GCalId = gEvent.Id;
                myPB.WorkplaceId = wpid;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/publicbooking/Store");
                throw;
            }

            SmsCampaign newSmsCampaign;
            int myUserId=0;
            if (appointmentAutoApprove == true)
            {
                // obvesti stranko prek sms (TODO: naredi prek service)
                newSmsCampaign = new SmsCampaign();
                newSmsCampaign.Company.Id = myPB.Location.CompanyId;
                newSmsCampaign.LocationId = myPB.LocationId;
                newSmsCampaign.PublicBookingId = myPB.Id;
                var myUser = _context.Users.Where(c => c.CompanyId == myPB.Location.CompanyId).OrderBy(o => o.Id).AsNoTracking().First();
                myUserId= myUser.Id;
                newSmsCampaign.AppUser.Id = myUserId;

                newSmsCampaign.MsgTxt = @"Vaš termin je bil sprejet! Naročeni ste " + myPB.Start.Value.ToString("dd.MM.yyyy") + " ob " + myPB.Start.Value.ToString("HH:mm") + ". Lep pozdrav! " + myPB.Location.Name;
                if (string.IsNullOrEmpty(myPB.Location.Tel) == false)
                { newSmsCampaign.MsgTxt += Environment.NewLine + "Za več informacij nas pokličite na " + myPB.Location.Tel; }
                var mySmsInfo = new SmsCounter(newSmsCampaign.MsgTxt);

                newSmsCampaign.MsgSegments = mySmsInfo.Messages;
                newSmsCampaign.Name = "PublicBookingConfimation";
                newSmsCampaign.Recipients.Add(new SmsMsg(myPB.Mobile, (myPB.Client != null ? myPB.Client.Id : 0)));

                newSmsCampaign.SendAfter = DateTime.Now;
                newSmsCampaign.ApprovedAt = DateTime.Now;


                _context.Attach(newSmsCampaign.Company);
                _context.Attach(newSmsCampaign.AppUser);
                _context.SmsCampaigns.Add(newSmsCampaign);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "/api/publicbooking/ create PublicBookingAutoApprove SmsCampaign");
                    throw;
                }
            }
            if (bool.Parse(SettingsHelper.getSetting(_context, myPB.Location.CompanyId, myPB.Location.Id, "PublicBooking_AlertMeWithSMS", "true")) == true)
            {
                //alert about new appointment
                //(TODO: naredi prek service)
                try
                {
                    newSmsCampaign = new SmsCampaign();
                    newSmsCampaign.Company.Id = myPB.Location.CompanyId;
                    newSmsCampaign.LocationId = myPB.LocationId;
                    newSmsCampaign.PublicBookingId = myPB.Id;
                    if (appointmentAutoApprove == false)
                    { myUserId = _context.Users.Where(c => c.CompanyId == myPB.Location.CompanyId).OrderBy(o => o.Id).AsNoTracking().First().Id; }
                    newSmsCampaign.AppUser.Id = myUserId;
                    newSmsCampaign.MsgTxt = @"Novo narocilo prek spleta! Poglej v https://KdajBi.si";
                    var mySmsInfo = new SmsCounter(newSmsCampaign.MsgTxt);

                    newSmsCampaign.MsgSegments = mySmsInfo.Messages;
                    newSmsCampaign.Name = "PublicBookingAlert";
                    newSmsCampaign.Recipients.Add(new SmsMsg(myPB.Location.Tel.Replace(" ",""), 0));

                    newSmsCampaign.SendAfter = DateTime.Now;
                    newSmsCampaign.ApprovedAt = DateTime.Now;

                    if (appointmentAutoApprove == false)
                    {
                        _context.Attach(newSmsCampaign.Company);
                        _context.Attach(newSmsCampaign.AppUser);
                    }
                    else
                    { _context.Entry(newSmsCampaign.AppUser).State = EntityState.Detached;
                        _context.Entry(_context.Users.Find(myUserId)).State = EntityState.Detached;
                        _context.Entry(_context.Companies.Find(myPB.Location.CompanyId)).State = EntityState.Detached;
                        _context.Attach(newSmsCampaign.Company);
                        _context.Attach(newSmsCampaign.AppUser);
                    }

                    _context.SmsCampaigns.Add(newSmsCampaign);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "/api/publicbooking/ create PublicBookingAlert SmsCampaign");
                    throw;
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/publicbooking/ save PublicBookingAlert SmsCampaign");
                throw;
            }
             

            return Ok();
        }

        /// <summary>
        /// returns a list of services UsedInClientBooking
        /// </summary>
        /// <param name="lid">Location id (contains location)</param>
        /// <param name="wpid">Workplace Id </param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/publicbooking/getservices/{lid}/{wpid}")]
        public async Task< ActionResult<List<Service>>> getservices(long lid, long wpid)
        {
            //get public booking services
           
            //var myPB = await _context.PublicBookings.FindAsync(pbid);
            var services = await _context.Services.Include(g=>g.ServiceGroup).Where(s => s.UsedInClientBooking == true && s.LocationId == lid).OrderBy(s=>s.ServiceGroup.SortPosition).ToListAsync();

            //remove all services not done by specified workplace (if supplied)
            if (wpid > 0)
            {
                var wpExServices = await _context.WorkplaceExcludedServices.Where(w => w.WorkplaceId == wpid).ToListAsync();
                if (wpExServices.Count > 0)
                {
                    var wpServices = services.Where(p => wpExServices.All(p2 => p2.ServiceId != p.Id)).ToList();
                    return Ok(wpServices);
                }
            }
            else
            {
                //remove all services not done by any workplace 
                var allWPIds = await _context.Workplaces.Where(wp => wp.LocationId == lid).Select(wp=>wp.Id).ToListAsync();
                var wpExServices = await _context.WorkplaceExcludedServices.Where(w => allWPIds.Contains(w.WorkplaceId)).ToListAsync();
                if (wpExServices.Count > 0)
                {
                    var neki = wpExServices.GroupBy(x => x.ServiceId).Where(n => n.Count() == allWPIds.Count()).ToList();
                    var wpServices = services.Where(p => neki.All(p2 => p2.Key != p.Id)).ToList();
                    return Ok(wpServices);
                }
            }
            return Ok(services);
        }

        #region "confirmation"
        /// <summary>
        /// confirms public booking (sets Status=1), sends client notification (via sms)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("/api/publicbooking-confirmation/{id}")]
        public async Task<ActionResult<List<TimeSlot>>> Update(int id)
        {
            // validate timeslot
            var myPB = await _context.PublicBookings.Include(pb => pb.Workplace)
                .Include(pb => pb.Location)
                .Include(pb => pb.Workplace)
                .Include(pb => pb.Client)
                .Include(pb => pb.Service)
                .Where(pb => pb.Id == id).FirstOrDefaultAsync();
            if (myPB == null) { return NotFound(); }

            myPB.Status = 1; //confirmed
            myPB.UpdatedDate = DateTime.Now;
            try
            {
                 await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/publicbooking-confirmation");
                throw;
            }

            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                myGoogleHelper.UpdateEvent(
                    (myPB.Client!=null?myPB.Client.FullName:"") +" ("+myPB.Mobile+ ") - " + (myPB.Service != null ? myPB.Service.Name : ""),
                    myPB.Workplace.GoogleCalendarID,
                    myPB.GCalId
                );
            }

            // obvesti stranko prek sms (TODO: naredi prek service)
            SmsCampaign newSmsCampaign = new SmsCampaign();
            newSmsCampaign.Company.Id = _CurrentUserCompanyID();
            newSmsCampaign.LocationId = myPB.LocationId;
            newSmsCampaign.PublicBookingId = myPB.Id;
            newSmsCampaign.AppUser.Id = _CurrentUserID();

            newSmsCampaign.MsgTxt = @"Vaš termin je bil potrjen! Naročeni ste " + myPB.Start.Value.ToString("dd.MM.yyyy") + " ob " + myPB.Start.Value.ToString("HH:mm") + ". Lep pozdrav! "+myPB.Location.Name;
            if (string.IsNullOrEmpty(myPB.Location.Tel) == false)
            { newSmsCampaign.MsgTxt += Environment.NewLine + "Za več informacij nas pokličite na " + myPB.Location.Tel; }
            var mySmsInfo = new SmsCounter(newSmsCampaign.MsgTxt);

            newSmsCampaign.MsgSegments = mySmsInfo.Messages;
            newSmsCampaign.Name = "PublicBookingConfimation";
            newSmsCampaign.Recipients.Add(new SmsMsg(myPB.Mobile, (myPB.Client != null ? myPB.Client.Id : 0)));

            newSmsCampaign.SendAfter = DateTime.Now;
            newSmsCampaign.ApprovedAt = DateTime.Now;


            _context.Attach(newSmsCampaign.Company);
            _context.Attach(newSmsCampaign.AppUser);
            _context.SmsCampaigns.Add(newSmsCampaign);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/publicbooking-confirmation");
                throw;
            }

            return Ok();
        }

        /// <summary>
        /// rejects public booking (sets Status=2), client is not notified
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("/api/publicbooking-confirmation/{id}")]
        public async Task<ActionResult<List<TimeSlot>>> Destroy(int id)
        {
            var myPB = await _context.PublicBookings.Include(pb => pb.Workplace)
                .Include(pb => pb.Workplace)
                .Where(pb => pb.Id == id).FirstOrDefaultAsync();
            if (myPB == null) { return NotFound(); }

            myPB.Status = 2;
            myPB.UpdatedDate = DateTime.Now;
            myPB.UpdatedUserID = _CurrentUserID();


            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                myGoogleHelper.DeleteEvent(
                    myPB.Workplace.GoogleCalendarID,
                    myPB.GCalId
                );
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/publicbooking-confirmation");
                throw;
            }

            return Ok();
        }
        #endregion
    }
}
