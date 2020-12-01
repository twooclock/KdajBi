using KdajBi.Core;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

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
            return int.Parse(User.Claims.First(i => i.Type == "CompanyId").Value);
        }
    }
}
