using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KdajBi.Web.Controllers
{
    [Authorize(Roles = "Super,Admin")]
    [Controller]
    public class WorkplaceScheduleController : _BaseController
    {
        public WorkplaceScheduleController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }


        [Route("/wps/{wpid}/{scheduletype}")]
        public IActionResult wps(long wpid, long scheduletype)
        {
            //edit schedule for a workplace wpid (default scheduleType=0 Alldays)
            bool alternateWeeks = (SettingsHelper.getSetting(_context, _CurrentUserCompanyID(), null, "cbEmployee_AlternatingWeeks", "false")) == "true";
            if (alternateWeeks == true && scheduletype == 0) { scheduletype++; }
            if (alternateWeeks == false && scheduletype != 0) { scheduletype=0; }
            vmWorkplace myVM = new vmWorkplace();
            myVM.Workplace = _context.Workplaces.Where(w => w.Id == wpid).SingleOrDefault();
            myVM.Workplace.WorkplaceSchedules = _context.WorkplaceSchedules.Include(s => s.Schedule).Where(wps => wps.WorkplaceId == wpid).ToList();
            myVM.Location = _context.Locations.Include(s => s.Schedule).Where(l => l.Id == myVM.Workplace.LocationId).SingleOrDefault();
            myVM.Location.Schedule = _context.Schedules.Find(myVM.Location.ScheduleId);
            var events = new List<FullCalendar.rEventShow>();
            var events2 = new List<FullCalendar.rEventShow>();
            myVM.cboScheduleTypeHTML(scheduletype, alternateWeeks);
            
            Schedule mySchedule = myVM.Workplace.ScheduleByType(scheduletype);
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
            devents = FullCalendar.CreateREventsFromREventShow(scheduletype, events, "blue");

            if (scheduletype != 0)
            {
                //add other schedule (odd,even)
                long other = ((scheduletype==1)?2:1);
                mySchedule = myVM.Workplace.ScheduleByType(other);
                if (mySchedule == null)
                {
                    //create default events from location schedule
                    events2= FullCalendar.getWeekrEventShowFromSchedule(myVM.Location.Schedule);
                }
                else
                {
                    //create events from selected schedule type
                    if (mySchedule.EventsJson == "")
                    {
                        events2= FullCalendar.getWeekrEventShowFromSchedule(mySchedule);
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
            myVM.calWEvents = Newtonsoft.Json.JsonConvert.SerializeObject(events.ToArray()).Replace(@"""title"":null,","").Replace(@",""display"":null","");
            myVM.calBGEvents = Newtonsoft.Json.JsonConvert.SerializeObject(devents.ToArray());
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

            

            myVM.Token = _GetToken();
            myVM.UserUIShow = _UserUIShow();

            return View(myVM);
        }

        

    }
}
