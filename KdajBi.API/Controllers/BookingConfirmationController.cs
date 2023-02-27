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

        public BookingConfirmationController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<AppointmentTokenController> logger,
            IEmailSender emailSender,
            ICalendarV3Provider calendarV3Provider
            )
            : base(context, userManager, signInManager, logger, emailSender)
        {
            _calendarV3Provider = calendarV3Provider;
        }

        [HttpGet("/api/booking-confirmation")]
        public ActionResult<List<TimeSlot>> Index()
        {
            List<AppointmentToken> BookingConfirmation = _context.AppointmentTokens
                .Where(b => b.CompanyId == _CurrentUserCompanyID() && b.GCalId !=null && b.Status==0)
                .ToList();

            return Ok(BookingConfirmation);
        }

        [HttpPut("/api/booking-confirmation/{id}")]
        public ActionResult<List<TimeSlot>> Update(int id)
        {

            // validate timeslot
            AppointmentToken bookingConfirmation = _context.AppointmentTokens
                .Include(b => b.Client)
                .Include(b => b.Location)
                .Include(b => b.Workplace)
                .Where(b => b.CompanyId == _CurrentUserCompanyID())
                .FirstOrDefault(b => b.Id == id);

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

            // obvesti stranko prek sms (TODO: naredi prek service)
            SmsCampaign newSmsCampaign = new SmsCampaign();
            newSmsCampaign.Company.Id = _CurrentUserCompanyID();
            newSmsCampaign.LocationId = bookingConfirmation.LocationId;
            newSmsCampaign.AppUser.Id = _CurrentUserID();

            newSmsCampaign.MsgTxt = @"Vaš termin je bil potrjen! Naročeni ste "+ bookingConfirmation.Start.Value.ToString("dd.MM.yyyy")+" ob "+ bookingConfirmation.Start.Value.ToString("HH:mm")+". Lep pozdrav! ";
            if (string.IsNullOrEmpty(bookingConfirmation.Location.Tel) == false)
            { newSmsCampaign.MsgTxt += Environment.NewLine + "Za več informacij nas pokličite na " + bookingConfirmation.Location.Tel; }
            var mySmsInfo = new SmsCounter(newSmsCampaign.MsgTxt);

            newSmsCampaign.MsgSegments = mySmsInfo.Messages;

            newSmsCampaign.Name = "AppointmentConfimation";

            newSmsCampaign.Recipients.Add(new SmsMsg(bookingConfirmation.Client.Mobile , bookingConfirmation.Client.Id));

            newSmsCampaign.SendAfter = DateTime.Now;
            newSmsCampaign.ApprovedAt = DateTime.Now;


            _context.Attach(newSmsCampaign.Company);
            _context.Attach(newSmsCampaign.AppUser);
            _context.SmsCampaigns.Add(newSmsCampaign);
            try
            {
                 _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "/api/appointment-tokens Error");
                throw;
            }

            return Ok();
        }

        [HttpDelete("/api/booking-confirmation/{id}")]
        public ActionResult<List<TimeSlot>> Destroy(int id)
        {
            AppointmentToken bookingConfirmation = _context.AppointmentTokens
                .Include(b => b.Client)
                .Include(b => b.Workplace)
                .Where(b => b.CompanyId == _CurrentUserCompanyID())
                .FirstOrDefault(b => b.Id == id);

            bookingConfirmation.Status = 2;
            bookingConfirmation.UpdatedUserID = _CurrentUserID();

            using (CalendarV3Helper myGoogleHelper = _calendarV3Provider.GetHelper())
            {
                myGoogleHelper.DeleteEvent(
                    bookingConfirmation.Workplace.GoogleCalendarID,
                    bookingConfirmation.GCalId
                );
            }
            _context.SaveChanges();

            return Ok();
        }
    }
}
