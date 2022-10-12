using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using KdajBi.GoogleHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;

namespace KdajBi.Web.Controllers
{
    [Controller]
    public class BookingController : Controller
    {
        protected readonly ApplicationDbContext _context;
        public BookingController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
        {
            _context = context;
        }
        
        [AllowAnonymous]
        [Route("/booking/{token}")]
        public IActionResult Index(string token)
        {
            AppointmentToken appointmentToken = _context.AppointmentTokens.Include(s => s.Company).FirstOrDefault(x => x.Token == token);
            vmBooking vm = new vmBooking();
            vm.token = appointmentToken;
            return View(vm);
        }

        [AllowAnonymous]
        [Route("/booking-successful")]
        public IActionResult Success()
        {
            return View();
        }
    }
}
