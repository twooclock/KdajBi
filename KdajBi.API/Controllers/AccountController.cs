using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using KdajBi.Core.Models;
using KdajBi.Models;
using KdajBi.API.SecurityCore.Services;
using KdajBi.API.Controllers.Resources;
using Microsoft.Extensions.Logging;

namespace KdajBi.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr, IAuthenticationService authenticationService)
        {
            userManager = userMgr;
            signInManager = signinMgr;
            _authenticationService = authenticationService;

        }


        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            //return RedirectToAction("Index", "Home");
            return Redirect("~/LandingPage/index.html");
        }



    }
}
