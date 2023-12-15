using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;

namespace KdajBi.Web.Controllers
{
    //[Authorize(Roles = "Super,Admin")]
    [Controller]
    public class ClientsController : _BaseController
    {

        public ClientsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }




        [Route("/Clients/List")]
        public IActionResult List()
        {
            if (LocationIsMine(DefaultLocationId()))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId()).OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { value = p.Id, label = p.FullName }).ToList()).Replace(@"\", @"\\");
                myVM.Token = _GetToken();
                myVM.UserUIShow = _UserUIShow();
                return View(myVM);
            }
            return NotFound();
        }


        [Route("/Clients")]
        public IActionResult Index()
        {
            if (LocationIsMine(DefaultLocationId()))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId()).OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { value = p.Id, label = (p.FullName + " " + p.Mobile) }).ToList()).Replace(@"\", @"\\");
                var myLocation = _context.Locations.Include(s => s.Schedule).Include(w => w.Workplaces).FirstOrDefault(x => x.Id == DefaultLocationId());
                if (myLocation != null)
                {
                    //load google calendars
                    var gt = _CurrentUserGooToken();
                    if (gt != null)
                    {
                        using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                        {
                            var cals = service.getCalendars().Items;
                            if (cals != null)
                            {
                                for (int i = myLocation.Workplaces.Count - 1; i >= 0; i--)
                                {
                                    var item = myLocation.Workplaces.ElementAt(i);
                                    if (cals.Where(c => c.Id == item.GoogleCalendarID).Count() == 0) { item.GoogleCalendarID = null; }
                                    if (item.GoogleCalendarID != null)
                                    {
                                        myVM.GoogleCalendars.Add(new Tuple<string, string, long>(item.GoogleCalendarID, item.Name, item.Id));
                                    }
                                    else
                                    {
                                        myLocation.Workplaces.Remove(item);
                                    }

                                }
                            }
                            else
                            {
                                //didnt get any google calendars
                                //either user has any
                                //or google error occured
                                _logger.LogInformation("No google calendars for " + User.Identity.Name);
                            }
                        }
                    }
                }
                myVM.Token = _GetToken();
                myVM.UserUIShow = _UserUIShow();
                return View(myVM);
            }
            return NotFound();
        }

        [Route("/Clients/Notification")]
        public IActionResult Notification()
        {
            if (LocationIsMine(DefaultLocationId()))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId() && c.AllowsSMS == true && c.Mobile != "").OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { Id = p.Id, FullName = p.FullName, ct = "#" + String.Join("#", p.ClientTags.Select(t => t.TagId.ToString())) + "#" }).ToList()).Replace(@"\", @"\\");
                myVM.Token = _GetToken();
                myVM.UserUIShow = _UserUIShow();
                return View(myVM);
            }
            return NotFound();
        }

    }
}
