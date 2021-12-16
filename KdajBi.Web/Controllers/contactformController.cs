
using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace KdajBi.Web.Controllers
{
    public class ContactForm
    {
        public string name { get; set; }
        public string email { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
        
    }


    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class contactformController : _BaseController
    {
        
        public contactformController(ApplicationDbContext context, UserManager<AppUser> _userManager, SignInManager<AppUser> _signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, _userManager, _signInManager, logger, emailSender, apiTokenProvider)
        {

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/api/contactform")]
        public async Task<ActionResult<ContactForm>> contactform(ContactForm cForm)
        {

            _logger.LogInformation(string.Format("New contact name:'{0}', email:'{1}', subject:'{2}', message:'{3}' ", cForm.name, cForm.email, cForm.subject, cForm.message));

            await _emailSender.SendEmailAsync(cForm.email, _emailSender.emailSettings().AdminMail, "new contact!", string.Format("New contact name:'{0}', email:'{1}', subject:'{2}', message:'{3}' ", cForm.name, cForm.email, cForm.subject, cForm.message));

            return Json("OK");
        }
    }
}