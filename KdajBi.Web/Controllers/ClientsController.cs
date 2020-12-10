using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

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
            vmClient myVM = new vmClient();
            myVM.Clients = _context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID()).ToList();
            myVM.Token = _GetToken();
            return View(myVM);
        }

        [Route("/Clients/Index2")]
        public IActionResult Index2()
        {
            vmClient myVM = new vmClient();
            myVM.Clients = _context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID()).ToList();
            myVM.Token = _GetToken();
            return View(myVM);
        }
    }
}
