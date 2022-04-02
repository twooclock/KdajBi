using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [ApiController]
    public class BookingController : _BaseController
    {
        public BookingController(
            ApplicationDbContext context, 
            UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, 
            ILogger<AppointmentTokenController> logger, 
            IEmailSender emailSender
            )
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }

        [AllowAnonymous]
        [HttpGet("/api/booking/{token}")]
        public ActionResult<AppointmentToken> Get(
            string token,
            [FromQuery] DateTime? date = null)
        {
            if (date == null) {
                date = DateTime.Today;
            }
            
            AppointmentToken appointmentToken = _context.AppointmentTokens.Include(s => s.Company).FirstOrDefault(x => x.Token == token);
            
            //We need to generate list of available appointments here

            // String[,] appointmets = new String[,] {};
            String[,] appointmets = new String[,] {
                {"10:00", "14:00"},
                {"14:00", "18:00"},
                {"15:00", "19:00"},
                {"16:00", "20:00"}
            };
            return Ok(appointmets);
        }
    }
}
