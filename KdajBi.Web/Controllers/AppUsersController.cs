﻿using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KdajBi.Web.Controllers
{
    [Authorize(Roles = "Super,Admin")]
    [Controller]
    public class AppUsersController : _BaseController
    {
        public AppUsersController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider) 
            :base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }


        [Route("/users")]
        public IActionResult Index()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = _GetToken();
            vmModel.UserUIShow = _UserUIShow();

            return View(vmModel);
        }

        
    }
}
