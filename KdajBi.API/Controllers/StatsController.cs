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
using System.Globalization;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Drawing;

namespace KdajBi.API.Controllers
{
    [ApiController]
    public class StatsController : _BaseController
    {
        protected readonly ICalendarV3Provider _calendarV3Provider;

        public StatsController(
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

        private async Task<bool> LocationIsMine(long id)
        {
            return (await _context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).CountAsync() == 1);
        }

        /// <summary>
        /// returns a list of public booking events
        /// </summary>
        /// <param name="locationid"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("/api/stats/PublicBookingsTimeline/{locationid}")]
        public async Task<JsonResult> PublicBookingsTimeline(long locationid, [FromQuery] DateTime? date = null)
        {
            //TODO:
            //date = new DateTime(2024, 6, 22);
            
            if (await LocationIsMine(locationid) == false) { return Json(""); }
            if (date.HasValue == false) { date = new DateTime(); }
            DateTime nextDay = date.Value.AddDays(1);
            var myPB = await _context.PublicBookings.Include(c => c.Client).Where(w => w.LocationId == locationid && (w.CreatedDate.Value >= date && w.CreatedDate.Value < nextDay)).ToListAsync();
            var smsi = await _context.SmsCampaigns.Where(w => w.LocationId == locationid && w.PublicBookingId !=null && (w.Date.Value >= date && w.Date.Value < nextDay)).ToListAsync();
            List<dynamic> retval = new List<dynamic>();
            foreach (var item in myPB)
            {
                string pbType = "red";
                //determine PB.type
                if (item.Authorized.HasValue && item.CreatedDate > DateTime.Now.AddMinutes(-30)) { pbType = "yellow";  }
                if (item.WorkplaceId.HasValue && item.Status == 1) { pbType = "green";  }
                var newPB = new { pbid = item.Id, type= pbType, date = item.CreatedDate, client = (item.Client!=null?item.Client.FullName + " (" + item.Client.Mobile + ")" : "Nova stranka? ("+ item.Mobile + ")") , Events = new List<object>() };
                var newEvent = new {  date = item.CreatedDate, text = "Vpis številke", cssClass ="" }; newPB.Events.Add(newEvent);
                if (item.Authorized.HasValue)
                { newEvent = new { date = item.Authorized, text = "Vpis PINa", cssClass = "" };   newPB.Events.Add(newEvent); }
                if (item.WorkplaceId.HasValue && item.Status==0)
                { newEvent = new { date = item.UpdatedDate, text = "Čaka na potrditev", cssClass = "" }; newPB.Events.Add(newEvent); }
                if (item.WorkplaceId.HasValue && item.Status == 1)
                { newEvent = new { date = item.UpdatedDate, text = "Potrditev naročila", cssClass = "" }; newPB.Events.Add(newEvent); }
                if (item.WorkplaceId.HasValue && item.Status == 2)
                { newEvent = new { date = item.UpdatedDate, text = "Zavrnitev naročila", cssClass = "" }; newPB.Events.Add(newEvent); }
                foreach (var smsItem in smsi.FindAll(s => s.PublicBookingId == item.Id).ToList())
                {
                    string myCssClass = "";
                    DateTime? myDatum;
                    if (smsItem.SentError > 0 || smsItem.SentAt == null) { myDatum = smsItem.Date; myCssClass = "text-danger"; }
                    else { myDatum = smsItem.SentAt; }
                    if (smsItem.Name == "PublicBookingAuthorization") { newEvent = new { date = myDatum, text = "SMS PIN za prijavo", cssClass = myCssClass }; newPB.Events.Add(newEvent); }
                    if (smsItem.Name == "PublicBookingAlert") { newEvent = new { date = myDatum, text = "SMS Novo naročilo prek spleta!", cssClass = myCssClass }; newPB.Events.Add(newEvent); }
                    if (smsItem.Name == "PublicBookingConfimation") { newEvent = new { date = myDatum, text = "SMS Vaš termin je bil potrjen!", cssClass = myCssClass }; newPB.Events.Add(newEvent); }
                }

                retval.Add(newPB);

            }

            var sortedList = retval.AsEnumerable().OrderBy(x => x.date);
            return Json(sortedList);


        }

        
        
    }
}
