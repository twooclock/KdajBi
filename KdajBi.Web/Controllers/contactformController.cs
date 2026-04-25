
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
        public string code { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
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
            _logger.LogInformation(string.Format("New contact code:'{0}', name:'{1}', phone:'{2}', email:'{3}', subject:'{4}', message:'{5}' ", cForm.code, cForm.name, cForm.phone, cForm.email, cForm.subject, cForm.message));
            switch (cForm.code)
            {
                case "ZATE":
                    await _emailSender.SendEmailAsync(cForm.email, "salon.zate.info@gmail.com", "novo povpraševanje", string.Format("Ime:'{0}', Telefon:'{1}', ePošta:'{2}', sporočilo:'{3}' ", cForm.name, cForm.phone, cForm.email,  cForm.message));
                    break;
                default:
                    await _emailSender.SendEmailAsync(cForm.email, _emailSender.emailSettings().AdminMail, "new contact!", string.Format("New contact name:'{0}', email:'{1}', subject:'{2}', message:'{3}' ", cForm.name, cForm.email, cForm.subject, cForm.message));
                    break;
            }
            
            return Json("OK");
        }
    }
}