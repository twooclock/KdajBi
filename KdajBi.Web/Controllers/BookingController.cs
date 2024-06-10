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
            AppointmentToken appointmentToken = _context.AppointmentTokens.Include(l=>l.Location).Include(s => s.Company).FirstOrDefault(x => x.Token == token);
            
            vmBooking vm = new vmBooking();
            vm.token = appointmentToken;
            vm.PublicBoooking_MaxDays = SettingsHelper.getSetting(_context, appointmentToken.CompanyId, appointmentToken.LocationId, "PublicBooking_MaxDays", 0);
            vm.PublicBooking_AllowCurrentDay = SettingsHelper.getSetting(_context, appointmentToken.CompanyId, appointmentToken.LocationId, "PublicBooking_AllowCurrentDay", true);
            vm.PublicBooking_AlertMeWithSMS = SettingsHelper.getSetting(_context, appointmentToken.CompanyId, appointmentToken.LocationId, "PublicBooking_AlertMeWithSMS", true);
            vm.PublicBoooking_CSS = SettingsHelper.getSetting(_context, appointmentToken.CompanyId, appointmentToken.LocationId , "PublicBooking_CSS", "");

            return View(vm);
        }

        [AllowAnonymous]
        [Route("/booking-successful/{token}")]
        public IActionResult Success(string token)
        {
            AppointmentToken appointmentToken = _context.AppointmentTokens.FirstOrDefault(x => x.Token == token);
            vmBooking vm = new vmBooking();
            vm.PublicBoooking_CSS = SettingsHelper.getSetting(_context, appointmentToken.CompanyId, appointmentToken.LocationId, "PublicBooking_CSS", "");

            return View(vm);
        }
    }
}
