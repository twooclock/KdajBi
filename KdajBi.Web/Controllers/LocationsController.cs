﻿using KdajBi.Core;
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
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using KdajBi.GoogleHelper;
using System.Threading.Tasks;

namespace KdajBi.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Controller]
    public class LocationsController : _BaseController
    {
        public LocationsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }
        public async Task<IActionResult> Index()
        {
            long curruserCompanyId = _CurrentUserCompanyID();
            //show current user locations
            List<Location> currUserLocations = new List<Location>();
            var v = from a in _context.Locations select a;
            //v = v.Include(c => c.Company);
            v = v.Where(c => c.CompanyId == curruserCompanyId);
            currUserLocations = v.ToList();
            vmLocations myVM = new vmLocations();
            myVM.Locations = currUserLocations;
            myVM.Token = await _GetToken();
            myVM.UserUIShow = await _UserUIShow();
            return View(myVM);
        }

        [Route("/location/{id}")]
        public async Task<IActionResult> Location(long id)
        {
            try
            {
                vmLocation myVM = new vmLocation();
                myVM.Location = _context.Locations.Include(l=>l.Schedule).Where( x => x.Id == id).First();
                myVM.Location.Schedule = _context.Schedules.Find(myVM.Location.ScheduleId);

                if (myVM.Location != null)
                {
                    //create location schedule events  
                    var events = new List<FullCalendar.rEventShow>();
                    if (string.IsNullOrEmpty(myVM.Location.Schedule.EventsJson))
                    { 
                        events = FullCalendar.getWeekrEventShowFromSchedule(myVM.Location.Schedule);
                        myVM.calWEvents = Newtonsoft.Json.JsonConvert.SerializeObject(events.ToArray());
                    }
                    else
                    { myVM.calWEvents = myVM.Location.Schedule.EventsJson; }
                    

                    //fill google calendars
                    var gt = await _CurrentUserGooToken();
                    if (gt != null)
                    {
                        using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                        {
                            foreach (var item in service.getCalendars().Items)
                            {
                                myVM.GoogleCalendars.Add(item.Id, item.Summary);
                            }
                        }
                    }
                    myVM.Token = await _GetToken();
                    myVM.UserUIShow = await _UserUIShow();
                    return View(myVM);
                }
                else
                { return new NotFoundResult(); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error /location/{id}");
                throw;
            }
        }
    }
}
