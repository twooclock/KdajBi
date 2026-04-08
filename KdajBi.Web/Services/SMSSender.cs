using KdajBi.Core;
using KdajBi.Core.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KdajBi.Web.Services
{
    public class SystemSMSSettings
    {
        public long SenderCompanyId { get; set; }
        public long SenderLocationId { get; set; }
        public int SenderAppUserId { get; set; }
        public string DefaultCampaignName { get; set; }
        public string DefaultRecipientMobileNumber   { get; set; }
    }
    public interface ISMSSender
    {
        SystemSMSSettings systemSMSSettings();
        bool EnqueueSMS(long p_CompanyId, long p_LocationId, long p_PublicBookingId, int p_AppUserId, string p_MsgTxt, string p_CampaignName, string p_Recipient, long p_ClientId);
        bool EnqueueSystemSMS(string p_MsgTxt, string CampaignName = "", string RecipientMobileNumber = "");
    }



    public class SMSSender : ISMSSender
    {
        private readonly SystemSMSSettings _systemSMSSettings;
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger _logger;

        SystemSMSSettings ISMSSender.systemSMSSettings()
        {
            return _systemSMSSettings;
        }
        public SMSSender(ApplicationDbContext context, ILogger<SMSSender> logger, IOptions<SystemSMSSettings> systemSMSSettings)
        {
            _context = context;
            _logger = logger;
            _systemSMSSettings = systemSMSSettings.Value;
        }

        public bool EnqueueSMS(long p_CompanyId, long p_LocationId, long p_PublicBookingId, int p_AppUserId, string p_MsgTxt, 
            string p_CampaignName, string p_RecipientMobileNumber, long p_RecipientClientId)
        {
            if (bool.Parse(SettingsHelper.getSetting(_context, p_CompanyId, p_LocationId, "SMS_removeNonGSM7", "false")) == true)
            { p_MsgTxt = Core.Utils.ReplaceNonGSM7Chars(p_MsgTxt); }

            var newSmsCampaign = new SmsCampaign
            {
                CompanyId = p_CompanyId,
                UserId = p_AppUserId,
                LocationId = p_LocationId,
                PublicBookingId = p_PublicBookingId,
                MsgTxt = p_MsgTxt,
                MsgSegments = new SmsCounter(p_MsgTxt).Messages,
                Name = p_CampaignName,
                SendAfter = DateTime.Now,
                ApprovedAt = DateTime.Now
            };

            newSmsCampaign.Recipients.Add(
                new SmsMsg(p_RecipientMobileNumber, p_RecipientClientId)
            );

            // Attach navigation properties if they were auto-initialized
            // Check if already tracked before attaching
            if (newSmsCampaign.Company != null)
            {
                var trackedCompany = _context.Companies.Local
                    .FirstOrDefault(c => c.Id == p_CompanyId);

                if (trackedCompany != null)
                {
                    // Already tracked, use the tracked instance
                    newSmsCampaign.Company = trackedCompany;
                }
                else
                {
                    // Not tracked, attach it
                    newSmsCampaign.Company.Id = p_CompanyId;
                    _context.Attach(newSmsCampaign.Company);
                }
            }

            if (newSmsCampaign.AppUser != null)
            {
                var trackedUser = _context.Users.Local
                    .FirstOrDefault(u => u.Id == p_AppUserId);

                if (trackedUser != null)
                {
                    // Already tracked, use the tracked instance
                    newSmsCampaign.AppUser = trackedUser;
                }
                else
                {
                    // Not tracked, attach it
                    newSmsCampaign.AppUser.Id = p_AppUserId;
                    _context.Attach(newSmsCampaign.AppUser);
                }
            }
            _context.SmsCampaigns.Add(newSmsCampaign);

            try
            {
                 _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnqueueSMS");
                return false;
            }
            return true;
        }

        //for notifying system admin
        public bool EnqueueSystemSMS(string p_MsgTxt, string CampaignName = "", string RecipientMobileNumber = "")
        {
            if (string.IsNullOrWhiteSpace(CampaignName)) { CampaignName = _systemSMSSettings.DefaultCampaignName; }
            if (string.IsNullOrWhiteSpace(RecipientMobileNumber)) { RecipientMobileNumber = _systemSMSSettings.DefaultRecipientMobileNumber;}

            var newSmsCampaign = new SmsCampaign
            {
                CompanyId = _systemSMSSettings.SenderCompanyId,
                UserId = _systemSMSSettings.SenderAppUserId,
                LocationId = _systemSMSSettings.SenderLocationId,
                MsgTxt = p_MsgTxt,
                MsgSegments = new SmsCounter(p_MsgTxt).Messages,
                Name = CampaignName,
                SendAfter = DateTime.Now,
                ApprovedAt = DateTime.Now,
                Recipients = new List<SmsMsg> 
                {
                    new SmsMsg
                    {
                        Recipient = RecipientMobileNumber,
                        ClientId = 0 
                    }
                }
            };


            // Attach navigation properties if they were auto-initialized
            // Check if already tracked before attaching
            if (newSmsCampaign.Company != null)
            {
                var trackedCompany = _context.Companies.Local
                    .FirstOrDefault(c => c.Id == _systemSMSSettings.SenderCompanyId);

                if (trackedCompany != null)
                {
                    // Already tracked, use the tracked instance
                    newSmsCampaign.Company = trackedCompany;
                }
                else
                {
                    // Not tracked, attach it
                    newSmsCampaign.Company.Id = _systemSMSSettings.SenderCompanyId;
                    _context.Attach(newSmsCampaign.Company);
                }
            }

            if (newSmsCampaign.AppUser != null)
            {
                var trackedUser = _context.Users.Local
                    .FirstOrDefault(u => u.Id == _systemSMSSettings.SenderAppUserId);

                if (trackedUser != null)
                {
                    // Already tracked, use the tracked instance
                    newSmsCampaign.AppUser = trackedUser;
                }
                else
                {
                    // Not tracked, attach it
                    newSmsCampaign.AppUser.Id = _systemSMSSettings.SenderAppUserId;
                    _context.Attach(newSmsCampaign.AppUser);
                }
            }


            _context.SmsCampaigns.Add(newSmsCampaign);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while EnqueueSystemSMS for campaign {CampaignName}", CampaignName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in EnqueueSystemSMS for campaign {CampaignName}", CampaignName);
                return false;
            }

            return true;
        }
    }
}