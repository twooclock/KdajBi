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
        public IActionResult Index()
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
            myVM.Token = _GetToken();
            return View(myVM);
        }

        [Route("/location/{id}")]
        public IActionResult Location(long id)
        {
            vmLocation myVM = new vmLocation();
            myVM.Location = _context.Locations.Include(s=>s.Schedule).FirstOrDefault(x => x.Id == id);

            if (myVM.Location != null)
            {
                myVM.Token = _GetToken();
                return View(myVM); }
            else
            { return new NotFoundResult(); }
        }
    }
}
