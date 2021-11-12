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
                //TODO: check expiration and renew
                return JsonConvert.DeserializeObject<GoogleAuthToken>(_context.UserClaims.First(i => i.ClaimType == "GooToken" && i.UserId == _CurrentUserID()).ClaimValue);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "_CurrentUserGooToken error");
                return null;
            }
        }
    }
}
