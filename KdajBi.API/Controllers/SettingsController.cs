﻿using AutoMapper;
using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.dtoModels.Requests;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private IConfiguration _config;
        public SettingsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<ClientsController> logger, IEmailSender emailSender, IMapper mapper, IConfiguration config)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            _mapper = mapper; _config = config;
        }


        [HttpPost]
        [Route("/api/Settings/Save/{LocationId?}")]
        public JsonResult PostSave(long? locationid, Dictionary<string, string> p_settings)
        {
            Setting settingIndb;
            foreach (var item in p_settings)
            {
                if (locationid.HasValue == true)
                { settingIndb = _context.Settings.SingleOrDefault(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId==locationid && c.Key == item.Key); }
                else
                { settingIndb = _context.Settings.SingleOrDefault(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == null && c.Key == item.Key); }
                if (settingIndb == null)
                {
                    Setting newSetting = new Setting();
                    newSetting.UserId = _CurrentUserID();
                    newSetting.CreatedUserID = _CurrentUserID();
                    newSetting.CompanyId = _CurrentUserCompanyID();
                    newSetting.LocationId = locationid;
                    newSetting.Key = item.Key;
                    newSetting.Value = item.Value;
                    _context.Settings.Add(newSetting);
                }
                else
                {
                    settingIndb.UserId = _CurrentUserID();
                    settingIndb.UpdatedUserID = _CurrentUserID();
                    settingIndb.CompanyId = _CurrentUserCompanyID();
                    settingIndb.LocationId = locationid;
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
            if (p_settings.ContainsKey("SMS_GOO_Cals") == true)
            {
                //ensure google service user has appropriate permissions to read calendars set
                var gt = _CurrentUserGooToken();
                if (gt != null)
                {
                    using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                    {
                        if (service.EnsureReadPermissionsForService(p_settings["SMS_GOO_Cals"], _config.GetSection("GoogleSettings")["GooServiceAccount"]) == false)
                        {return Json("Not all calendar access permissions were set!"); }
                    }
                }

            }
            return Json("OK");
        }



        [HttpPost]
        [Route("/api/Settings/Load/{LocationId?}")]
        public JsonResult PostLoad(long? locationid, Dictionary<string, string> p_settings)
        {
            SettingsHelper.getSettings(_context, _CurrentUserCompanyID(), locationid, p_settings);
            return Json(p_settings);
        }


        [HttpPost("/api/Settings/ChangePassword/")]
        public JsonResult ChangePassword(List<string> changePass)
        {
            string currPass=SettingsHelper.getSetting(_context, _CurrentUserCompanyID(), null, "AdminPass", "");
            if (Utils.CreateMD5( changePass[0]) != (currPass == "" ? KdajBi.Core.Utils.CreateMD5("") : currPass)) 
            { 
                return Json("Napačno trenutno geslo!");
            }
            else
            {
                try
                {
                    SettingsHelper.saveSetting(_context, _CurrentUserID(), _CurrentUserCompanyID(), null, "AdminPass", Utils.CreateMD5(changePass[1]));
                }
                catch (Exception)
                {
                    return Json("Napaka!");
                }
            }
            return Json("OK");
        }

    }
}
