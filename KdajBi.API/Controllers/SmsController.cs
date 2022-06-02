using AutoMapper;
using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
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
    //[Authorize(Roles = "Super, Admin")]
    [ApiController]
    public class SmsController : _BaseController
    {

        private readonly ILogger<SmsController> _logger;
        private IMapper _mapper;
        private ISmsInfo _smsInfo;
        public SmsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<SmsController> logger, IEmailSender emailSender, IMapper mapper, ISmsInfo smsInfo)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            _logger = logger;
            _mapper = mapper;
            _smsInfo = smsInfo;
        }


        [HttpPost]
        [Route("/api/Sms/SmsTable/{LocationId?}")]
        public JsonResult SmsTable(long? locationid, [FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;

            var v = from a in _context.SmsCampaigns select a;
            if (locationid.HasValue == true)
            { v = v.Where(w => w.Company.Id == _CurrentUserCompanyID() && w.LocationId== locationid); }
            else
            { v = v.Where(w => w.Company.Id == _CurrentUserCompanyID()); }
            v = v.Include(w => w.Recipients);
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        [HttpPost]
        [Route("/api/Sms/SmsMsgTable/{id}")]
        public JsonResult SmsMsgTable(long Id, [FromBody] DataTableAjaxPostModel param)
        {
            //TODO:check is mine campaign
            int recordsTotal = 0;
            var v = from a in _context.SmsMsgs select a;
            v = v.Where(w => w.SmsCampaignId == Id);
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        [HttpPost("/api/Sms/QueueSmsCampaign")]
        public async Task<ActionResult<dtoSmsCampaigin>> PostQueueSmsCampaign(dtoSmsCampaigin p_SmsCampaigin)
        {
            if (ModelState.IsValid)
            {
                SmsCampaign newSmsCampaign = GetSmsCampaignFromSmsc(p_SmsCampaigin);

                _context.Attach(newSmsCampaign.Company);
                _context.Attach(newSmsCampaign.AppUser);
                _context.SmsCampaigns.Add(newSmsCampaign);
                string AppBaseUrl = Request.Scheme + @"://" + Request.Host + Request.PathBase;
                string myWhen = "";
                if (newSmsCampaign.SendAfter.HasValue == true)
                {
                    if (newSmsCampaign.SendAfter.Value < DateTime.Now)
                    { myWhen = "takoj"; }
                    else
                    {
                        string dowName = "";
                        switch (newSmsCampaign.SendAfter.Value.ToLocalTime().Date.DayOfWeek)
                        {
                            case DayOfWeek.Sunday:
                                dowName = "v nedeljo";
                                break;
                            case DayOfWeek.Monday:
                                dowName = "v ponedeljek";
                                break;
                            case DayOfWeek.Tuesday:
                                dowName = "v torek";
                                break;
                            case DayOfWeek.Wednesday:
                                dowName = "v sredo";
                                break;
                            case DayOfWeek.Thursday:
                                dowName = "v četrtek";
                                break;
                            case DayOfWeek.Friday:
                                dowName = "v petek";
                                break;
                            case DayOfWeek.Saturday:
                                dowName = "v soboto";
                                break;
                            default:
                                dowName = "?";
                                break;
                        }

                        if (newSmsCampaign.SendAfter.Value.Date == DateTime.UtcNow.Date)
                        { myWhen = "danes"; }
                        if (newSmsCampaign.SendAfter.Value.Date == DateTime.UtcNow.AddDays(1).Date)
                        { myWhen = "jutri"; }
                        if (newSmsCampaign.SendAfter.Value.Date > DateTime.UtcNow.AddDays(1).Date)
                        { myWhen = newSmsCampaign.SendAfter.Value.Date.ToString("dd.MM.yyyy"); }
                        myWhen += " (" + dowName + ")";
                        myWhen += " ob " + newSmsCampaign.SendAfter.Value.ToLocalTime().ToShortTimeString();
                    }
                }
                else
                { myWhen = "Takoj"; }
                try
                {
                    await _context.SaveChangesAsync();
                    //_logger.LogInformation("kampanija shranjena, sedaj pa še pošljem mail na naslov " + _CurrentUserEmail());
                    //notify user
                    string myMail = "<p>Pozdravljeni! <br />SMS sporočila zahtevajo vašo pozornost.<br />";
                    myMail += "Če potrdite, bo " + myWhen + " poslanih " + newSmsCampaign.RecipientsCount.ToString() + " sporočil:<br />";
                    myMail += "<pre>" + newSmsCampaign.MsgTxt + "</pre>";
                    myMail += @"Prosimo, <a href=""" + AppBaseUrl + @"/api/Sms?guid=" + newSmsCampaign.Guid.ToString() + @"&action=approve"" target=""_blank"">POTRDITE</a> ";
                    myMail += @"    ali    ";
                    myMail += @"<a href=""" + AppBaseUrl + @"/api/Sms?guid=" + newSmsCampaign.Guid.ToString() + @"&action=cancel"" target=""_blank"">PREKLIČITE</a> ";
                    myMail += @" pošiljanje.<br /> Hvala.</p>";

                    await _emailSender.SendEmailAsync(_CurrentUserEmail(), "SMS obveščanje", myMail);
                    //_logger.LogInformation("mail poslan...?");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "/api/Sms/QueueSmsCampaign Error");
                    throw;
                }
            }

            return Json("OK");

        }

        [HttpPost("/api/Sms/SmsInfo")]
        public async Task<ActionResult<dtoSmsCampaigin>> PostSmsInfo(dtoSmsCampaigin p_SmsCampaigin)
        {
            int ClientSmsLimit = 0;
            long recipientsCount = 0;
            if (ModelState.IsValid)
            {
                SmsCampaign newSmsCampaign = GetSmsCampaignFromSmsc(p_SmsCampaigin);
                //get sms limit info
                ClientSmsLimit = _smsInfo.SmsLimitInfo("KB" + _CurrentUserCompanyID().ToString(), _CurrentUserCompanyTaxID());
                recipientsCount = newSmsCampaign.RecipientsCount;

            }

            return Json(new { limit = ClientSmsLimit, recipients = recipientsCount });

        }

        [HttpGet]
        [AllowAnonymous]
        [Produces("text/html")]
        //[Route("/api/Sms/Decide/{guid}/{action}")]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetDecide(string guid, string action)
        {
            List<SmsCampaign> myCampaigns;
            var myCampaign = _context.SmsCampaigns.Include(s => s.Company).Where(c => c.Guid.ToString() == guid).FirstOrDefault();
            if (myCampaign != null && guid.Length > 0)
            {
                string AppBaseUrl = Request.Scheme + @"://" + Request.Host + Request.PathBase;
                string myHTML = @"<html><head><meta http-equiv=""refresh"" content=""5;url='https://kdajbi.si'"" />";

                switch (action.ToUpper())
                {
                    case "APPROVE":
                        myCampaign.ApprovedAt = DateTime.Now;
                        _context.Entry(myCampaign).State = EntityState.Modified;
                        _context.Entry(myCampaign).Property(p => p.RecipientsCount).IsModified = false;
                        await _context.SaveChangesAsync();
                        myHTML += "<title>Pošiljanje potrjeno</title></head>";
                        myHTML += "<body><h1>Potrjeno! <br />Sporočila bodo poslana.</h1></body></html>";
                        return Content(myHTML, "text/html", Encoding.UTF8);
                    case "CANCEL":
                        myCampaign.CanceledAt = DateTime.Now;
                        _context.Entry(myCampaign).State = EntityState.Modified;
                        _context.Entry(myCampaign).Property(p => p.RecipientsCount).IsModified = false;
                        await _context.SaveChangesAsync();
                        myHTML += "<title>Pošiljanje preklicano</title></head>";
                        myHTML += "<body><h1>Preklicano! <br />Sporočila ne bodo poslana.</h1></body></html>";
                        return Content(myHTML, "text/html", Encoding.UTF8);
                    case "APPROVEALL":
                        //get all GOO campaigns for companyid and date
                        //except ones that were individualy approved/canceled
                        myCampaigns = _context.SmsCampaigns.Where(c => c.Name == "GOO" && c.Company.Id == myCampaign.Company.Id && c.Date.Value.Date == myCampaign.Date.Value.Date && c.CanceledAt == null && c.ApprovedAt == null).ToList();
                        _logger.LogInformation("APPROVEALL " + myCampaigns.Count.ToString() + " GOO sms campaigns for " + myCampaign.Company.Name + " ("+guid+") on " + myCampaign.Date.Value.Date.ToString("dd.MM.yyyy"));
                        foreach (var item in myCampaigns)
                        {
                            item.ApprovedAt = DateTime.Now;
                            _context.Entry(item).State = EntityState.Modified;
                            _context.Entry(item).Property(p => p.RecipientsCount).IsModified = false;
                        }
                        await _context.SaveChangesAsync();

                        myHTML += "<title>Pošiljanje vseh potrjeno</title></head>";
                        myHTML += "<body><h1>Potrjeno! <br />Vsa sporočila bodo poslana.</h1></body></html>";
                        return Content(myHTML, "text/html", Encoding.UTF8);
                    case "CANCELALL":
                        //get all GOO campaigns for companyid and date
                        //except ones that were individualy approved/canceled
                        myCampaigns = _context.SmsCampaigns.Where(c => c.Name == "GOO" && c.Company.Id == myCampaign.Company.Id && c.Date.Value.Date == myCampaign.Date.Value.Date && c.CanceledAt==null && c.ApprovedAt==null).ToList();
                        _logger.LogInformation("CANCELALL " + myCampaigns.Count.ToString() + " GOO sms campaigns for " + myCampaign.Company.Name + " (" + guid + ") on " + myCampaign.Date.Value.Date.ToString("dd.MM.yyyy"));
                        foreach (var item in myCampaigns)
                        {
                            item.CanceledAt = DateTime.Now;
                            _context.Entry(item).State = EntityState.Modified;
                            _context.Entry(item).Property(p => p.RecipientsCount).IsModified = false;
                        }
                        await _context.SaveChangesAsync();
                        myHTML += "<title>Pošiljanje vseh preklicano</title></head>";
                        myHTML += "<body><h1>Preklicano! <br />Sporočila ne bodo poslana.</h1></body></html>";
                        return Content(myHTML, "text/html", Encoding.UTF8);
                    default:
                        return Content("<html><h1>Brezveze.</h1></html>", "text/html", Encoding.UTF8);
                }

            }
            else
            { return NotFound(); }
        }
        private SmsCampaign GetSmsCampaignFromSmsc(dtoSmsCampaigin p_SmsCampaigin)
        {
            SmsCampaign newSmsCampaign = new SmsCampaign();
            newSmsCampaign.Company.Id = _CurrentUserCompanyID();
            newSmsCampaign.LocationId = p_SmsCampaigin.LocationId;
            newSmsCampaign.AppUser.Id = _CurrentUserID();
            newSmsCampaign.MsgTxt = p_SmsCampaigin.MsgTxt;
            newSmsCampaign.SendAfter = p_SmsCampaigin.SendAfter;
            newSmsCampaign.Name = "";


            switch (p_SmsCampaigin.CampaignType)
            {
                case 0: //individual recipients (ClientIDs)
                    foreach (string item in p_SmsCampaigin.Recipients)
                    {
                        Client client = _context.Clients.Where(a => a.CompanyId == _CurrentUserCompanyID() && a.Id == long.Parse(item) && a.Mobile != null && a.AllowsSMS == true).SingleOrDefault();
                        if (client != null)
                        {
                            newSmsCampaign.Recipients.Add(new SmsMsg(client.Mobile));
                        }
                    }
                    break;
                case 1: //recipients by location
                    foreach (string item in p_SmsCampaigin.Recipients)
                    {
                        foreach (Client client in _context.Clients.Where(a => a.CompanyId == _CurrentUserCompanyID() && a.LocationId == long.Parse(item) && a.Mobile != null && a.AllowsSMS == true))
                        {
                            newSmsCampaign.Recipients.Add(new SmsMsg(client.Mobile));
                        }
                    }
                    break;
                case 2: //recipients by sex
                    foreach (Client client in _context.Clients.Where(a => a.CompanyId == _CurrentUserCompanyID() && a.Sex == p_SmsCampaigin.Recipients.ToArray()[0] && a.Mobile != null && a.AllowsSMS == true))
                    {
                        newSmsCampaign.Recipients.Add(new SmsMsg(client.Mobile));
                    }
                    break;
                default:
                    break;
            }
            return newSmsCampaign;
        }


        private bool ClientExists(long id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
        private bool ClientIsMine(long id)
        {
            return (_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1);
        }
        private bool LocationIsMine(long id)
        {
            return (_context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1);
        }
    }
}
