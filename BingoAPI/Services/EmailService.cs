using BingoAPI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class EmailService : IEmailService
    {
        private SmtpClient client;
        private MailAddress mailFrom;
        private ApplicationEmailSettings _emailSettings;

        public EmailService(IOptions<ApplicationEmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            client = new SmtpClient(_emailSettings.SmtpClient);
            client.Credentials = new System.Net.NetworkCredential(_emailSettings.EmailAddress, _emailSettings.Password);
            client.Port = _emailSettings.Port;
            client.EnableSsl = _emailSettings.SSL;
            mailFrom = new MailAddress(_emailSettings.EmailAddress);
        }


        public async Task<bool> SendEmail(string receiver, string subject, string message)
        {
            MailAddress mailTo = new MailAddress(receiver);
            MailMessage mailMessage = new MailMessage(mailFrom, mailTo);
            mailMessage.Body = message;
            mailMessage.Subject = subject;

            await client.SendMailAsync(mailMessage);

            return true;
        }
    }
}
