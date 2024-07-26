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
using System.Threading.Tasks;

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
        public async Task<IActionResult> AppMessages()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = await _GetToken();
            vmModel.UserUIShow = await _UserUIShow();
            return View(vmModel);
        }

        [Route("/AppMessage/{id}")]
        public async Task<IActionResult> AppMessage(long Id)
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = await _GetToken();
            vmModel.UserUIShow = await _UserUIShow();
            vmModel.Id = Id;
            return View(vmModel);
        }

        

    }
}
