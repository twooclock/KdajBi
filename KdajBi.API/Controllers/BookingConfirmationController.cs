using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using KdajBi.Core.dtoModels.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [ApiController]
    public class BookingConfirmationController : _BaseController
    {
        protected readonly ICalendarV3Provider _calendarV3Provider;
        protected readonly ISMSSender _smsSender;
        public BookingConfirmationController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<AppointmentTokenController> logger,
            IEmailSender emailSender,
            ICalendarV3Provider calendarV3Provider,
            ISMSSender smsSender
            )
            : base(context, userManager, signInManager, logger, emailSender)
        {
            _calendarV3Provider = calendarV3Provider;
            _smsSender = smsSender;
        }

        [HttpGet("/api/booking-confirmation")]
        public async Task<ActionResult<List<TimeSlot>>> Index()
        {
            List<AppointmentToken> BookingConfirmation = await _context.AppointmentTokens
                .Where(b => b.CompanyId == _CurrentUserCompanyID() && b.GCalId !=null && b.Status==0)
                .ToListAsync();

            return Ok(BookingConfirmation);
        }

        [HttpPut("/api/booking-confirmation/{id}")]
        public async Task<ActionResult<List<TimeSlot>>> Update(int id)
        {

            // validate timeslot
            AppointmentToken bookingConfirmation = await _context.AppointmentTokens
                .Include(b => b.Client)
                .Include(b => b.Location)
                .Include(b => b.Workplace)
                .Where(b => b.CompanyId == _CurrentUserCompanyID())
                .FirstOrDefaultAsync(b => b.Id == id);

            bookingConfirmation.Status = 1;
            bookingConfirmation.UpdatedUserID = _CurrentUserID();

            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                myGoogleHelper.UpdateEvent(
                    bookingConfirmation.Client.FullName + " - " + bookingConfirmation.Service,
                    bookingConfirmation.Workplace.GoogleCalendarID,
                    bookingConfirmation.GCalId
                );
            }

            
            // obvesti stranko prek sms
            string MsgTxt = @"Vaš termin je bil potrjen! Naročeni ste " + bookingConfirmation.Start.Value.ToString("dd.MM.yyyy") + " ob " + bookingConfirmation.Start.Value.ToString("HH:mm") + ". Lep pozdrav! " + bookingConfirmation.Location.Name;
            if (string.IsNullOrEmpty(bookingConfirmation.Location.Tel) == false)
            { MsgTxt += Environment.NewLine + "Za več informacij nas pokličite na " + bookingConfirmation.Location.Tel; }
            var myUser = await _context.Users.Where(c => c.CompanyId == bookingConfirmation.Location.CompanyId).OrderBy(o => o.Id).AsNoTracking().FirstAsync();

            _smsSender.EnqueueSMS(bookingConfirmation.Location.CompanyId, bookingConfirmation.LocationId, null, bookingConfirmation.Id,
                    myUser.Id, MsgTxt, "AppointmentConfimation", bookingConfirmation.Client.Mobile, bookingConfirmation.Client.Id);
                
            
            try
            {
                  await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/appointment-tokens Error");
                throw;
            }

            return Ok();
        }

        [HttpDelete("/api/booking-confirmation/{id}")]
        public async Task<ActionResult<List<TimeSlot>>> Destroy(int id)
        {
            AppointmentToken bookingConfirmation = await _context.AppointmentTokens
                .Include(b => b.Client)
                .Include(b => b.Workplace)
                .Where(b => b.CompanyId == _CurrentUserCompanyID())
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bookingConfirmation == null) { return NotFound(); }
            
            bookingConfirmation.Status = 2;
            bookingConfirmation.UpdatedUserID = _CurrentUserID();
            bookingConfirmation.UpdatedDate = DateTime.Now;

            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                myGoogleHelper.DeleteEvent(
                    bookingConfirmation.Workplace.GoogleCalendarID,
                    bookingConfirmation.GCalId
                );
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/booking-confirmation");
                throw;
            }

            return Ok();
        }
    }
}
