using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;

namespace KdajBi.Web.Controllers
{
    //[Authorize(Roles = "Super,Admin")]
    [Controller]
    public class SmsController : _BaseController
    {

        public SmsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }


        [Route("/sms/Campaigns")]
        public IActionResult Campaigns()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = _GetToken();
            return View(vmModel);
        }

        [Route("/sms/Campaign/{id}")]
        public IActionResult Campaign(long Id)
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = _GetToken();
            vmModel.Id = Id;
            return View(vmModel);
        }

        [Route("/sms/Notification")]
        public IActionResult Notification()
        {
            if (LocationIsMine(DefaultLocationId()))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId() && c.AllowsSMS == true && c.Mobile != "").OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { Id = p.Id, FullName = p.FullName, ct = "#" + String.Join("#", p.ClientTags.Select(t => t.TagId.ToString())) + "#" }).ToList()).Replace(@"\", @"\\");
                myVM.Token = _GetToken();
                return View(myVM);
            }
            return NotFound();
        }

        [Route("/sms/OrderSms")]
        public IActionResult OrderSms()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = _GetToken();
            vmModel.Id= _context.SmsCampaigns.Where(s => s.CompanyId == _CurrentUserCompanyID() && s.SentAt > DateTime.Now.AddDays(-30)).Sum(a => a.MsgSegments * a.RecipientsCount);
            return View(vmModel);
        }
    }
}
