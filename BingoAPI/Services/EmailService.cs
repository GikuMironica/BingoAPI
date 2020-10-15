using BingoAPI.Models;
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
        private readonly IErrorService errorService;
        private SmtpClient client;
        private MailAddress mailFrom;
        private ApplicationEmailSettings _emailSettings;

        public EmailService(IOptions<ApplicationEmailSettings> emailSettings, IErrorService errorService)
        {
            _emailSettings = emailSettings.Value;
            client = new SmtpClient(_emailSettings.SmtpClient);
            client.Credentials = new System.Net.NetworkCredential(_emailSettings.EmailAddress, _emailSettings.Password);
            client.Port = _emailSettings.Port;
            client.EnableSsl = _emailSettings.SSL;
            mailFrom = new MailAddress(_emailSettings.Sender);
            this.errorService = errorService;
        }


        public async Task<bool> SendEmail(string receiver, string subject, string message)
        {
            MailAddress mailTo = new MailAddress(receiver);

            MailMessage mailMessage = new MailMessage(mailFrom, mailTo);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = message;
            mailMessage.Subject = subject;

            try
            {
                await client.SendMailAsync(mailMessage);
            }catch(Exception e)
            {
                var errorObj = new ErrorLog
                {
                    Date = DateTime.Now,
                    ExtraData = "Email could not be sent to "+receiver,
                    Message = e.Message
                };
                await errorService.AddErrorAsync(errorObj);
            }

            return true;
        }
    }
}
