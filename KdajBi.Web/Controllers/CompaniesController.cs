using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KdajBi.Web.Controllers
{
    [Authorize(Roles = "Super,Admin")]
    [Controller]
    public class CompaniesController : _BaseController
    {
        public CompaniesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }


        [Route("/companies")]
        public async Task<IActionResult> Index()
        {
            _BaseViewModel vmModel = new _BaseViewModel();
            vmModel.Token = await _GetToken();
            return View(vmModel);
        }

        [Route("/companies/mycompany")]
        public async Task<IActionResult> MyCompany()
        {
            vmCompany myVM = new vmCompany();
            myVM.Company = _context.Companies.Find(_CurrentUserCompanyID());
            if (myVM.Company != null)
            {
                myVM.Token = await _GetToken();
                return View(myVM);
            }
            else
            { return new NotFoundResult(); }
        }

    }
}
