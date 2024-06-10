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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [Authorize(Roles = "Super, Admin")]
    [ApiController]
    public class AppointmentTokenController : _BaseController
    {
        public AppointmentTokenController(
            ApplicationDbContext context, 
            UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, 
            ILogger<AppointmentTokenController> logger, 
            IEmailSender emailSender
            )
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }

        [HttpGet("/api/appointment-tokens")]
        public ActionResult<AppointmentToken> Get(
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 10)
        {
            var appointmentTokens = _context.AppointmentTokens.PageResult(page, perPage);
            return Ok(appointmentTokens);
        }

        [HttpGet("/api/appointment-tokens/{id}")]
        public async Task<ActionResult<AppointmentToken>> Show(long id)
        {
            var appointmentToken = await _context.AppointmentTokens.FindAsync(id);
            if (appointmentToken == null)
            {
                return NotFound();
            }
            return Ok(appointmentToken);
        }

        [HttpPost("/api/appointment-tokens")]
        public async Task<ActionResult<AppointmentToken>> Store(AppointmentTokenRequest appointmentTokenRequest)
        {
            Client client = _context.Clients.Where(a => a.CompanyId == _CurrentUserCompanyID() && a.Id == appointmentTokenRequest.ClientId && a.Mobile != null && a.AllowsSMS == true).SingleOrDefault();
            if (client != null)
            {
                AppointmentToken appointmentToken = new AppointmentToken();

                appointmentToken.Token = generateToken(); 
                appointmentToken.Service = appointmentTokenRequest.Service.Trim(); 
                appointmentToken.Minutes = appointmentTokenRequest.Minutes; 
                appointmentToken.CompanyId = _CurrentUserCompanyID(); 
                appointmentToken.LocationId = appointmentTokenRequest.LocationId; 
                appointmentToken.ClientId = appointmentTokenRequest.ClientId; 
                appointmentToken.AppUserId = _CurrentUserID();
                appointmentToken.WorkplaceId= appointmentTokenRequest.WorkplaceId;
                appointmentToken.CreatedDate = DateTime.Now;
                appointmentToken.CreatedUserID= _CurrentUserID();

                _context.AppointmentTokens.Add(appointmentToken);
                
                if (appointmentTokenRequest.SendSMS==true)
                { 
                    // obvesti stranko prek sms (appointmentToken.ClientId)
                    SmsCampaign newSmsCampaign = new SmsCampaign();
                    newSmsCampaign.Company.Id = _CurrentUserCompanyID();
                    newSmsCampaign.LocationId = appointmentTokenRequest.LocationId;
                    newSmsCampaign.AppUser.Id = _CurrentUserID();

                    newSmsCampaign.MsgTxt = @"Pozdravljeni! Naročite se lahko preko naslednje povezave: https://kdajbi.si/booking/" + appointmentToken.Token;
                    Location myLocation = _context.Locations.Find(appointmentTokenRequest.LocationId);
                    newSmsCampaign.MsgTxt += Environment.NewLine + @"Lep pozdrav! " + myLocation.Name;
                    if (string.IsNullOrEmpty(myLocation.Tel) == false)
                    { newSmsCampaign.MsgTxt += Environment.NewLine + "Za več informacij nas pokličite na " + myLocation.Tel; }
                    var mySmsInfo = new SmsCounter(newSmsCampaign.MsgTxt);
                    newSmsCampaign.MsgSegments = mySmsInfo.Messages;
                    newSmsCampaign.Name = "AppointmentLink";
                    newSmsCampaign.Recipients.Add(new SmsMsg(client.Mobile, appointmentToken.ClientId));

                    newSmsCampaign.SendAfter = DateTime.Now;
                    newSmsCampaign.ApprovedAt = DateTime.Now;
                    

                    _context.Attach(newSmsCampaign.Company);
                    _context.Attach(newSmsCampaign.AppUser);
                    _context.SmsCampaigns.Add(newSmsCampaign);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "/api/appointment-tokens Error");
                        throw;
                    }
                }
                return Ok(appointmentToken);
            }
            return NotFound();
        }



        [HttpDelete("/api/appointment-tokens/{id}")]
        public async Task<ActionResult<AppointmentToken>> Delete(long id)
        {
            var appointmentToken = await _context.AppointmentTokens.FindAsync(id);
            if (appointmentToken == null)
            {
                return NotFound();
            }

            _context.AppointmentTokens.Remove(appointmentToken);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string generateToken()
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnpqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
