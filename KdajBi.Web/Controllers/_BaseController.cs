using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using KdajBi.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace KdajBi.Web.Controllers
{
    public abstract class _BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly SignInManager<AppUser> _signInManager;
        protected readonly UserManager<AppUser> _userManager;
        protected readonly ILogger<_BaseController> _logger;
        protected readonly IEmailSender _emailSender;
        protected readonly IApiTokenProvider _apiTokenProvider;
        public _BaseController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<_BaseController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _apiTokenProvider = apiTokenProvider;


        }
        protected int _CurrentUserID()
        {
            return int.Parse(_userManager.GetUserId(User));
        }

        protected long _CurrentUserCompanyID()
        {
            return long.Parse(User.FindFirst("CompanyId").Value);
        }

        protected GoogleAuthToken _CurrentUserGooToken()
        {
            try
            {
                //check expiration and renew
                Claim gToken = User.FindFirst("GooToken");
                if (gToken == null)
                {
                    _logger.LogInformation("_CurrentUserGooToken claim User.FindFirst(GooToken) is null!");
                    return null;
                }
                var gooToken = JsonConvert.DeserializeObject<GoogleAuthToken>(gToken.Value);
                using (GoogleService service = new GoogleService(User.Identity.Name, gooToken))
                {
                    if (service.TokenWasRefreshed == true)
                    {
                        gooToken.access_token = service.googleAuthToken.access_token;
                        gooToken.expires_at = service.googleAuthToken.expires_at;
                        if (service.googleAuthToken.refresh_token != null) { gooToken.refresh_token = service.googleAuthToken.refresh_token; }
                        var strJson = JsonConvert.SerializeObject(gooToken);
                        var myClaim = new Claim("GooToken", strJson);
                        AppUser appUser = _userManager.FindByNameAsync(User.Identity.Name).Result;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "_CurrentUserGooToken error");
                return null;
            }
        }

        protected JwtToken _GetToken()
        {
            JwtToken retval;
            if (User.Identity.IsAuthenticated)
            {
                retval = _apiTokenProvider.GetToken(User.Identity.Name);
                if (retval != null)
                { return retval; }
                else
                {
                    retval = new JwtToken();
                    retval.AccessToken = "ERROR:No token for " + User.Identity.Name;
                    return retval;
                }
            }
            else
            {
                retval = new JwtToken();
                retval.AccessToken = "ERROR:User not authenticated";
                return retval;
            }
        }

        protected bool LocationIsMine(long locationId)
        {
            return (_context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == locationId).Count() == 1);
        }
        protected bool ClientIsMine(long clientId)
        {
            return (_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == clientId).Count() == 1);
        }

        protected long DefaultLocationId()
        {
            long retval = -1;
            string id = HttpContext.Request.Cookies[Utils.CookieNames.DefaultLocation];
            if (long.TryParse(id, out retval) == false)
            {
                id = Utils.GetCookieValueFromResponse(HttpContext.Response, Utils.CookieNames.DefaultLocation);
                if (long.TryParse(id, out retval) == false)
                { retval = _context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID()).FirstOrDefault().Id; }
            }
            if (LocationIsMine(retval) == false)
            { retval = _context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID()).FirstOrDefault().Id; }
            return retval;
        }

        protected List<string> _UserUIShow()
        {
            var retval= _context.Settings.Where(a => a.CompanyId == _CurrentUserCompanyID() && a.LocationId == null && a.Key == "UserUIShow").FirstOrDefault();
            if (retval == null) { 
                //return default menu for an ordinary user
                return new List<string>() {"Appointments","Clients", "ClientsList","SMSCampaigns" };
            }
            else { return JsonConvert.DeserializeObject<List<string>>(retval.Value); }
        }
    }
}
