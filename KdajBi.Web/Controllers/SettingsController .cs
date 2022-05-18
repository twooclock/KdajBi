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
using KdajBi.GoogleHelper;

namespace KdajBi.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Controller]
    public class SettingsController : _BaseController
    {
        public SettingsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }
        [Route("/settings/")]
        public IActionResult Index()
        {
            vmLocation myVM = new vmLocation();

            var gt = _CurrentUserGooToken();
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
            myVM.Token = _GetToken();
            return View(myVM);
        }

        
    }
}
