using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using KdajBi.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        protected async Task<GoogleAuthToken> _CurrentUserGooToken()
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
                        AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);
                        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
                        var existingClaim = claimsPrincipal.Claims.FirstOrDefault(r => r.Type == "GooToken");
                        if (existingClaim == null)
                        {
                            //add Google token to claims
                            await _userManager.AddClaimAsync(appUser, myClaim);
                        }
                        else
                        {
                            await _userManager.ReplaceClaimAsync(appUser, existingClaim, myClaim);
                        }
                        await _signInManager.SignInAsync(appUser, false, null); //to refresh user claims
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

        protected async Task<JwtToken> _GetToken()
        {
            JwtToken retval;
            if (User.Identity.IsAuthenticated)
            {
                retval = await _apiTokenProvider.GetToken(User.Identity.Name);
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

        protected async Task<bool> LocationIsMine(long locationId)
        {
            return (await _context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == locationId).CountAsync() == 1);
        }
        protected async Task<bool> ClientIsMine(long clientId)
        {
            return (await _context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == clientId).CountAsync() == 1);
        }

        protected async Task<long> DefaultLocationId()
        {
            long retval = -1;
            string id = HttpContext.Request.Cookies[Utils.CookieNames.DefaultLocation];
            if (long.TryParse(id, out retval) == false)
            {
                id = Utils.GetCookieValueFromResponse(HttpContext.Response, Utils.CookieNames.DefaultLocation);
                if (long.TryParse(id, out retval) == false)
                { retval = (await _context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID()).FirstOrDefaultAsync()).Id; }
            }
            if (await LocationIsMine(retval) == false)
            { retval = (await _context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID()).FirstOrDefaultAsync()).Id; }
            return retval;
        }

        protected async Task<List<string>> _UserUIShow()
        {
            var retval= await _context.Settings.Where(a => a.CompanyId == _CurrentUserCompanyID() && a.LocationId == null && a.Key == "UserUIShow").FirstOrDefaultAsync();
            if (retval == null) { 
                //return default menu for an ordinary user
                return new List<string>() {"Appointments","Clients", "ClientsList","SMSCampaigns" };
            }
            else { return JsonConvert.DeserializeObject<List<string>>(retval.Value); }
        }
    }
}
