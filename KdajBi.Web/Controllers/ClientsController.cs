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
    public class ClientsController : _BaseController
    {

        public ClientsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }




        [Route("/Clients")]
        public IActionResult Index()
        {
            if (LocationIsMine(DefaultLocationId()))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId()).OrderBy(o=>o.FirstName).ThenBy(o=>o.LastName).Select(p => new { value = p.Id, label = p.FullName }).ToList()).Replace(@"\", @"\\");
                myVM.Token = _GetToken();
                return View(myVM);
            }
            return NotFound();
        }


        [Route("/Clients/Index3")]
        public IActionResult Index3()
        {
            if (LocationIsMine(DefaultLocationId()))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId()).OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { value = p.Id, label = p.FullName }).ToList()).Replace(@"\", @"\\");
                myVM.Token = _GetToken();
                return View(myVM);
            }
            return NotFound();
        }

        [Route("/Clients/Notification")]
        public IActionResult Notification()
        {
            if (LocationIsMine(DefaultLocationId()))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == DefaultLocationId() && c.AllowsSMS == true && c.Mobile != "").OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { Id = p.Id, FullName = p.FullName,ct="#"+String.Join("#", p.ClientTags.Select(t=>t.TagId.ToString()))+ "#" }).ToList()).Replace(@"\", @"\\");
                myVM.Token = _GetToken();
                return View(myVM);
            }
            return NotFound();
        }

    }
}
