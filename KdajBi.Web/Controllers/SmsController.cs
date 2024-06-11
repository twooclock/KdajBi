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
using System.Threading.Tasks;

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
        public async Task<IActionResult> Campaigns()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = await _GetToken();
            vmModel.UserUIShow = await _UserUIShow();
            return View(vmModel);
        }

        [Route("/sms/Campaign/{id}")]
        public async Task<IActionResult> Campaign(long Id)
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = await _GetToken();
            vmModel.UserUIShow = await _UserUIShow();
            vmModel.Id = Id;
            return View(vmModel);
        }

        [Route("/sms/Notification")]
        public async Task<IActionResult> Notification()
        {
            long defLocId = await DefaultLocationId();
            if (await LocationIsMine(defLocId))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == defLocId && c.AllowsSMS == true && c.Mobile != "").OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { Id = p.Id, FullName = p.FullName, ct = "#" + String.Join("#", p.ClientTags.Select(t => t.TagId.ToString())) + "#" }).ToList()).Replace(@"\", @"\\");
                myVM.Token = await _GetToken();
                myVM.UserUIShow = await _UserUIShow();
                return View(myVM);
            }
            return NotFound();
        }

        [Route("/sms/OrderSms")]
        public async Task<IActionResult> OrderSms()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = await _GetToken();
            vmModel.UserUIShow = await _UserUIShow();
            vmModel.Id= _context.SmsCampaigns.Where(s => s.CompanyId == _CurrentUserCompanyID() && s.SentAt > DateTime.Now.AddDays(-30)).Sum(a => a.MsgSegments * a.RecipientsCount);
            return View(vmModel);
        }
    }
}
