﻿using System.Net;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace KdajBi
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailSettings
    {
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string SenderName { get; set; }
        public string SenderMail { get; set; }
        public string SenderPass { get; set; }



        public string AdminMail { get; set; }
        public bool UseSsl { get; set; }
        public bool UsePickupDirectory { get; set; }
        public bool RequiresAuthentication { get; set; }
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        protected readonly ILogger _logger;

        public EmailSender(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string p_To, string p_Subject, string p_HtmlMessage)
        {
            try
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderMail));

                mailMessage.To.Add(new MailboxAddress("", p_To));
                mailMessage.Subject = p_Subject;
                mailMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = p_HtmlMessage
                };

                using (SmtpClient client = new SmtpClient())
                {
                    client.Timeout = 20000;
                    //await client.SendMailAsync(mailMessage).ConfigureAwait(false);
                    await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, _emailSettings.UseSsl).ConfigureAwait(false);
                    await client.AuthenticateAsync(new NetworkCredential(_emailSettings.SenderMail, _emailSettings.SenderPass));
                    await client.SendAsync(mailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in SendEmailAsync("+p_To+","+p_Subject+"):"+p_HtmlMessage);

            }
        }
    }
}