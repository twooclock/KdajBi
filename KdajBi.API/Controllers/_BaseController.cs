using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

namespace KdajBi.API.Controllers
{
    public abstract class _BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly SignInManager<AppUser> _signInManager;
        protected readonly UserManager<AppUser> _userManager;
        protected readonly ILogger<_BaseController> _logger;
        protected readonly IEmailSender _emailSender;
        public _BaseController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<_BaseController> logger, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;


        }
        protected int _CurrentUserID()
        {
            return int.Parse(User.Claims.First(i => i.Type == "UserId").Value);
        }
        protected int _CurrentUserCompanyID()
        {
            return int.Parse(User.FindFirst("CompanyId").Value);
        }

        protected string _CurrentUserCompanyTaxID()
        {
            return User.FindFirst("CompanyTaxId").Value;
        }
        protected string _CurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        protected GoogleAuthToken _CurrentUserGooToken()
        {
            try
            {
                //before
                //return JsonConvert.DeserializeObject<GoogleAuthToken>(_context.UserClaims.First(i => i.ClaimType == "GooToken" && i.UserId == _CurrentUserID()).ClaimValue);
                //check expiration and renew
                var gooToken = JsonConvert.DeserializeObject<GoogleAuthToken>(_context.UserClaims.First(i => i.ClaimType == "GooToken" && i.UserId == _CurrentUserID()).ClaimValue);
                using (GoogleService service = new GoogleService(_CurrentUserEmail(), gooToken))
                {
                    if (service.TokenWasRefreshed == true)
                    {
                        gooToken.access_token = service.googleAuthToken.access_token;
                        gooToken.expires_at = service.googleAuthToken.expires_at;
                        if (service.googleAuthToken.refresh_token != null) { gooToken.refresh_token = service.googleAuthToken.refresh_token; }
                        var strJson = JsonConvert.SerializeObject(gooToken);
                        var myClaim = new Claim("GooToken", strJson);
                        AppUser appUser = _userManager.FindByNameAsync(_CurrentUserEmail()).Result;
                        var claimsPrincipal = _signInManager.CreateUserPrincipalAsync(appUser).Result;
                        var existingClaim = claimsPrincipal.Claims.FirstOrDefault(r => r.Type == "GooToken");
                        if (existingClaim == null)
                        {
                            //add Google token to claims
                            _ = _userManager.AddClaimAsync(appUser, myClaim).Result;
                        }
                        else
                        {
                            _ = _userManager.ReplaceClaimAsync(appUser, existingClaim, myClaim).Result;
                        }
                        _ = _signInManager.SignInAsync(appUser, false, null); //to refresh user claims
                    }
                }
                return gooToken;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "_CurrentUserGooToken error");
                return null;
            }
        }


    }
}
