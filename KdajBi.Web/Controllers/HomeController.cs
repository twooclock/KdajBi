using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KdajBi.Web.Controllers
{
    [Controller]
    public class HomeController : _BaseController
    {

        public HomeController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (LocationIsMine(DefaultLocationId()))
                    {
                        _BaseViewModel vmModel = new _BaseViewModel();
                        vmModel.Token = _GetToken();
                        vmModel.UserUIShow = _UserUIShow();
                        return View(vmModel);
                    }
                    else
                    { return NotFound(); }
                }
                else
                { return Redirect("~/LandingPage/index.html"); }
            }
            catch (Exception)
            {

                return Redirect("~/LandingPage/index.html");
            }


        }
        public async Task<IActionResult> NewUserTour()
        {

            //// Get User and a claims-based identity
            //var identity = new ClaimsIdentity(User.Identity);

            //var existingClaim = identity.FindFirst("Nadzornik");
            //if (existingClaim == null)
            //{
            //    identity.AddClaim(new Claim("Nadzornik", true.ToString()));
            //}
            

            //var authProperties = new AuthenticationProperties { IsPersistent = false };
            //await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity), authProperties);

            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = _GetToken();
            vmModel.UserUIShow = _UserUIShow();
            return View(vmModel);

        }

        public IActionResult Privacy()
        {
			_BaseViewModel vmModel = new _BaseViewModel();
			vmModel.Token = _GetToken();
			vmModel.UserUIShow = _UserUIShow();
			return View(vmModel);
        }

		public IActionResult Help()
		{
			_BaseViewModel vmModel = new _BaseViewModel();
			vmModel.Token = _GetToken();
			vmModel.UserUIShow = _UserUIShow();
			return View(vmModel);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
