using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using KdajBi.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using KdajBi.Models;
using System.Diagnostics;
using KdajBi.GoogleHelper;
using Google.Apis.Calendar.v3.Data;
using System.Globalization;
using Newtonsoft.Json;

namespace KdajBi.Web.Controllers
{
    [Controller]
    public class AppointmentsController : _BaseController
    {

        public AppointmentsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }

        [AllowAnonymous]
        public IActionResult Index(string? date)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (LocationIsMine(DefaultLocationId()))
                    {
                        long scheduletype = 0;
                        bool alternateWeeks = (SettingsHelper.getSetting(_context, _CurrentUserCompanyID(), null, "cbEmployee_AlternatingWeeks", "false")) == "true";
                        if (alternateWeeks == true) { scheduletype = 1; }
                        try
                        {
                            vmAppointments myVM = new vmAppointments();
                            myVM.Location = _context.Locations.Include(s => s.Schedule).Include(w => w.Workplaces).FirstOrDefault(x => x.Id == DefaultLocationId());
                            if (myVM.Location != null)
                            {
                                myVM.Location.Schedule = _context.Schedules.Find(myVM.Location.ScheduleId);
                                //load appropriate settings
                                myVM.Settings = new Dictionary<string, string>();
                                myVM.Settings.Add("SMS_AppointmentSMS", "false"); //mind missing sms settings if set to true!
                                myVM.Settings.Add("SMS_GOO_Msg", "Pozdravljeni! Naročeni ste <DANESJUTRI> <DATUM> ob <URA>. Veselimo se vašega obiska!");
                                SettingsHelper.getSettings(_context, _CurrentUserCompanyID(), DefaultLocationId(), myVM.Settings);
                                //load setting for null location
                                var globalSettings = new Dictionary<string, string>();
                                globalSettings.Add("cbUseSingleListOfServices", "false");
                                globalSettings.Add("cbAppointments_ShowTimetables", "false");
                                globalSettings.Add("cbEmployee_AlternatingWeeks", "false");
                                globalSettings.Add("AppointmentsSumServicesLength", "false");
                                globalSettings.Add("AppointmentsShow3Calendars", "false");
                                globalSettings.Add("AppointmentsRowHeight", "");
                                globalSettings.Add("AppointmentsMinColWidth", "");

                                SettingsHelper.getSettings(_context, _CurrentUserCompanyID(),null, globalSettings);
                                foreach (var item in globalSettings)
                                {
                                    myVM.Settings.Add(item.Key,item.Value);
                                }
                                //myVM.Settings.Add("cbUseSingleListOfServices", SettingsHelper.getSetting(_context, _CurrentUserCompanyID(), null, "cbUseSingleListOfServices", "false"));
                                //load google calendars
                                var gt = _CurrentUserGooToken();
                                if (gt != null)
                                {
                                    var services = _context.Services.Include(g => g.ServiceGroup).Where(c => c.CompanyId == _CurrentUserCompanyID()).ToList();
                                    var events = new List<FullCalendar.Event>();
                                    using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                                    {
                                        var cals = service.getCalendars().Items;
                                        if (cals != null)
                                        {
                                            for (int i = myVM.Location.Workplaces.Count - 1; i >= 0; i--)
                                            {
                                                var item = myVM.Location.Workplaces.ElementAt(i);
                                                if (cals.Where(c => c.Id == item.GoogleCalendarID).Count() == 0) { item.GoogleCalendarID = null; }
                                                //get calendar color
                                                foreach (var cal in cals)
                                                {
                                                    if (cal.Id == item.GoogleCalendarID) { item.GoogleCalendarColor = cal.BackgroundColor; }
                                                }
                                                if (item.GoogleCalendarID != null)
                                                {
                                                    myVM.GoogleCalendars.Add(new Tuple<string, string, long>(item.GoogleCalendarID, item.Name, item.Id));
                                                    //get calendar events
                                                    List<Event> calEvents = service.GetEvents(item.GoogleCalendarID, DateTime.Now);

                                                    foreach (var calEvent in calEvents)
                                                    {
                                                        //ignore all day events
                                                        if (calEvent.Start.DateTime != null && calEvent.End.DateTime != null)
                                                        {
                                                            var start = calEvent.Start.DateTime.Value;
                                                            var end = calEvent.End.DateTime.Value;
                                                            var newEvent = new FullCalendar.Event()
                                                            {
                                                                id = calEvent.Id,
                                                                resourceId = item.Id.ToString(),
                                                                title = calEvent.Summary,
                                                                start = start.ToString("yyyy-MM-ddTHH:mm:ss"),
                                                                end = end.ToString("yyyy-MM-ddTHH:mm:ss"),
                                                                allDay = false

                                                            };
                                                            if (calEvent.ExtendedProperties != null)
                                                            {
                                                                newEvent.extendedProps = (Dictionary<string, string>)calEvent.ExtendedProperties.Shared;
                                                                if (newEvent.extendedProps != null)
                                                                {

                                                                    if (newEvent.extendedProps.ContainsKey("notes"))
                                                                    {
                                                                        try
                                                                        {
                                                                            dynamic myNotes = JsonConvert.DeserializeObject<dynamic>(newEvent.extendedProps["notes"]);
                                                                            //if (myNotes[0].color != null)
                                                                            //{ newEvent.color = "#" + myNotes[0].color.Value; newEvent.textColor = Core.Utils.PickTextColorBasedOnBgColorSimple(myNotes[0].color.Value) ; }
                                                                            if (myNotes[0].value != null)
                                                                            {
                                                                                var storitev = services.FirstOrDefault(c => c.Id == long.Parse((string)(myNotes[0].value)));
                                                                                if (storitev!=null)
                                                                                { 
                                                                                    string barva = storitev.Color;
                                                                                    if (barva == null && storitev.ServiceGroup != null ) {
                                                                                        barva= storitev.ServiceGroup.Color;
                                                                                    }
                                                                                    if (barva != null) { newEvent.color = "#" + barva; }
                                                                                }
                                                                            }

                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            //newEvent.title = newEvent.title + "\r\n" + newEvent.extendedProps["notes"];
                                                                        }
                                                                    }
                                                                }

                                                            }
                                                            if (newEvent.color == null) { newEvent.color = item.GoogleCalendarColor; }
                                                            events.Add(newEvent);
                                                        }
                                                    }
													//get schedule bgEvents
													if (globalSettings["cbAppointments_ShowTimetables"] == "true") { 
														setBGEvents(myVM, item.Id, scheduletype);
													}
													
												}
                                                else
                                                {
                                                    myVM.Location.Workplaces.Remove(item);
                                                }
                                                
                                            }
                                        }
                                        else {
                                            //didnt get any google calendars
                                            //either user has any
                                            //or google error occured
                                            _logger.LogInformation("No google calendars for " + User.Identity.Name);
                                        }

                                    }

                                    myVM.AddcalEvents(Newtonsoft.Json.JsonConvert.SerializeObject(events.ToArray()));

                                }

                                Dictionary<string, string> mySettings = new Dictionary<string, string>();
                                mySettings.Add("cbUseSingleListOfClients", "false");
                                SettingsHelper.getSettings(_context, _CurrentUserCompanyID(), null, mySettings);
                                if (mySettings["cbUseSingleListOfClients"] == "false")
                                { myVM.ClientsJson = Newtonsoft.Json.JsonConvert.SerializeObject(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId()).OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { value = p.Id, label = (p.FullName + Convert.ToChar(160).ToString() + p.Mobile), mobile = p.Mobile, notes=p.AppointmentNotes }).ToList()).Replace(@"\", @"\\"); }
                                else
                                { myVM.ClientsJson = Newtonsoft.Json.JsonConvert.SerializeObject(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID()).OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { value = p.Id, label = (p.FullName + Convert.ToChar(160).ToString() + p.Mobile), mobile = p.Mobile, notes = p.AppointmentNotes }).ToList()).Replace(@"\", @"\\"); }

                                myVM.Token = _GetToken();
                                myVM.UserUIShow = _UserUIShow();
                                //myVM.Location.Workplaces= myVM.Location.Workplaces.OrderBy(s => s.SortPosition).ToList();

                                return View(myVM);
                            }
                            else
                            { return new NotFoundResult(); }
                        }
                        catch (Exception ex)
                        {
                            //TODO:expired google credentials?
                            _logger.LogError(ex, "Error AppointmentsController.Index (throwing next error)");
                            throw;
                        }
                    }
                    else
                    { return NotFound(); }
                }
                else
                {
                    _logger.LogError("Error User.identity is NOT Authenticated!");
                    return Redirect("~/LandingPage/index.html");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error /Appointments/index");
                //TODO:not all errors are solved with user logoff/logon! FIX!
                return Redirect("~/LandingPage/index.html");
            }


        }

        private void setBGEvents(vmAppointments p_myVM, long p_WorkplaceId, long p_scheduleType)
        {
            try
            {
                //edit schedule for a workplace wpid (default scheduleType=0 Alldays)
                vmWorkplace myVM = new vmWorkplace();
                myVM.Workplace = _context.Workplaces.Where(w => w.Id == p_WorkplaceId).SingleOrDefault();
                myVM.Workplace.WorkplaceSchedules = _context.WorkplaceSchedules.Include(s => s.Schedule).Where(wps => wps.WorkplaceId == p_WorkplaceId).ToList();
                myVM.Location = _context.Locations.Include(s => s.Schedule).Where(l => l.Id == myVM.Workplace.LocationId).SingleOrDefault();
                myVM.Location.Schedule = _context.Schedules.Find(myVM.Location.ScheduleId);
                var events = new List<FullCalendar.rEventShow>();
                var events2 = new List<FullCalendar.rEventShow>();

                Schedule mySchedule = myVM.Workplace.ScheduleByType(p_scheduleType);
                if (mySchedule == null)
                {
                    //create default events from location schedule
                    events = FullCalendar.getWeekrEventShowFromSchedule(myVM.Location.Schedule);
                }
                else
                {
                    //create events from selected schedule type
                    if (mySchedule.EventsJson == "")
                    {
                        events = FullCalendar.getWeekrEventShowFromSchedule(mySchedule);
                    }
                    else
                    { events = Newtonsoft.Json.JsonConvert.DeserializeObject<FullCalendar.rEventShow[]>(mySchedule.EventsJson).ToList(); }
                    myVM.ScheduleId = mySchedule.Id;
                }

                var devents = new List<FullCalendar.rEvent>();
                var d2events = new List<FullCalendar.rEvent>();
                devents = FullCalendar.CreateREventsFromREventShow(p_scheduleType, events, "blue");

                if (p_scheduleType != 0)
                {
                    //add other schedule (odd,even)
                    long other = ((p_scheduleType == 1) ? 2 : 1);
                    mySchedule = myVM.Workplace.ScheduleByType(other);
                    if (mySchedule == null)
                    {
                        //create default events from location schedule
                        events2 = FullCalendar.getWeekrEventShowFromSchedule(myVM.Location.Schedule);
                    }
                    else
                    {
                        //create events from selected schedule type
                        if (mySchedule.EventsJson == "")
                        {
                            events2 = FullCalendar.getWeekrEventShowFromSchedule(mySchedule);
                        }
                        else
                        {
                            events2 = Newtonsoft.Json.JsonConvert.DeserializeObject<FullCalendar.rEventShow[]>(mySchedule.EventsJson).ToList();
                        }

                    }
                    d2events = FullCalendar.CreateREventsFromREventShow(other, events2, "red");
                }



                //events.AddRange(events2);
                devents.AddRange(d2events);
                //add resourceId
                foreach (var item in devents)
                {
                    item.resourceId = p_WorkplaceId.ToString();
                    item.backgroundColor = "";
                }
                p_myVM.AddcalBGEvents(Newtonsoft.Json.JsonConvert.SerializeObject(devents.ToArray()));
                //add revents to weekdays
                WeekDay myWeekday;
                foreach (var item in devents)
                {
                    int idx = int.Parse(item.id[1].ToString());
                    if (myVM.WeekDays[idx] != null)
                    {
                        myWeekday = myVM.WeekDays[idx];
                        myWeekday.calEvents.Add(item);
                        myVM.WeekDays[idx] = myWeekday;
                    }
                    else
                    {
                        myWeekday = new WeekDay();
                        myWeekday.calEvents.Add(item);
                        myVM.WeekDays[idx] = myWeekday;
                    }
                }

                //set results
                resourceWD myRWD = new resourceWD();
                myRWD.resourceId = p_WorkplaceId;
                myRWD.WeekDays = myVM.WeekDays;
                p_myVM.resourcesWD.Add(myRWD);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error setBGEvents");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
