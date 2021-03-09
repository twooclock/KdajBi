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
    [Authorize(Roles = "Super, Admin")]
    [ApiController]
    public class SettingsController : _BaseController
    {
        private IMapper _mapper;
        public SettingsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<ClientsController> logger, IEmailSender emailSender, IMapper mapper)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            _mapper = mapper;
        }


        [HttpPost]
        [Route("/api/Settings/Save")]
        public JsonResult PostSave(Dictionary<string, string> p_settings)
        {
            foreach (var item in p_settings)
            {
                var settingIndb = _context.Settings.SingleOrDefault(c => c.CompanyId == _CurrentUserCompanyID() && c.Key == item.Key);
                if (settingIndb == null)
                {
                    Setting newSetting = new Setting();
                    newSetting.UserId = _CurrentUserID();
                    newSetting.CreatedUserID = _CurrentUserID();
                    newSetting.CompanyId = _CurrentUserCompanyID();
                    newSetting.Key = item.Key;
                    newSetting.Value = item.Value;
                    _context.Settings.Add(newSetting);
                }
                else
                {
                    settingIndb.UserId = _CurrentUserID();
                    settingIndb.UpdatedUserID = _CurrentUserID();
                    settingIndb.CompanyId = _CurrentUserCompanyID();
                    settingIndb.UpdatedDate = DateTime.Now;
                    settingIndb.Value = item.Value;
                    _context.Entry(settingIndb).State = EntityState.Modified;
                }
            }
            try
            {
                 _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in /api/Settings/Save");
                throw;
            }
            return Json("OK");
        }

        [HttpPost]
        [Route("/api/Settings/Load")]
        public JsonResult PostLoad(Dictionary<string, string> p_settings)
        {
            string[] keys = p_settings.Keys.ToArray();

            var mySettings = _context.Settings.Where(a => a.CompanyId == _CurrentUserCompanyID() && keys.Contains(a.Key)).ToList();
            foreach (var item in mySettings)
            {
                p_settings[item.Key] = item.Value;
            }
            return Json(p_settings);
        }



    }
}
