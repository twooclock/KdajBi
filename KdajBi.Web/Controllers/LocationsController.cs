using Google.Apis.Calendar.v3;
using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace KdajBi.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Controller]
    public class LocationsController : _BaseController
    {
        private IConfiguration _config;
        public LocationsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider, IConfiguration config)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
            _config = config;
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
                    var gCalIds = new List<string>();

                    if (gt != null)
                    {
                        using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                        {
                            foreach (var item in service.getCalendars().Items)
                            {
                                if (item.Id.Contains("holiday") == false) { 
                                    myVM.GoogleCalendars.Add(item.Id, item.Summary);
                                    gCalIds.Add(item.Id);
                                }
                            }
                        }
                    }
                    myVM.Token = await _GetToken();
                    myVM.UserUIShow = await _UserUIShow();

                    //ensure google service user has appropriate permissions to read calendars set
                    if (gt != null)
                    {
                        using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                        {
                            if (service.EnsureReadPermissionsForService(JsonConvert.SerializeObject(gCalIds), _config.GetSection("GoogleSettings")["GooServiceAccount"]) == false)
                            { 
                                _logger.LogError("Not all calendar access permissions for user "+ User.Identity.Name + " are set!"); }
                        }
                    }

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
