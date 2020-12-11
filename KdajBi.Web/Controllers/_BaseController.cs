using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        protected JwtToken _GetToken()
        {
            return _apiTokenProvider.GetToken(User.Identity.Name);
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
            if (long.TryParse(id,out retval)==false) { retval = -1; }
            return retval;
        }

       
    }
}
