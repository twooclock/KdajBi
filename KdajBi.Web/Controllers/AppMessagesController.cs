using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;

namespace KdajBi.Web.Controllers
{
    //[Authorize(Roles = "Super,Admin")]
    [Controller]
    public class AppMessagesController : _BaseController
    {

        public AppMessagesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }


        [Route("/AppMessages")]
        public IActionResult AppMessages()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = _GetToken();
            return View(vmModel);
        }

        [Route("/AppMessage/{id}")]
        public IActionResult AppMessage(long Id)
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = _GetToken();
            vmModel.Id = Id;
            return View(vmModel);
        }

        

    }
}
