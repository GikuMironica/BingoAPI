using BingoAPI.Models;
using BingoAPI.Options;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IErrorService _errorService;
        private readonly SmtpClient _client;
        private readonly MailAddress _mailFrom;
        private readonly ApplicationEmailSettings _emailSettings;

        public EmailService(IOptions<ApplicationEmailSettings> emailSettings, IErrorService errorService)
        {
            _emailSettings = emailSettings.Value;
            _client = new SmtpClient(_emailSettings.SmtpClient);
            _client.Credentials = new System.Net.NetworkCredential(_emailSettings.EmailAddress, _emailSettings.Password);
            _client.Port = _emailSettings.Port;
            _client.EnableSsl = _emailSettings.SSL;
            _mailFrom = new MailAddress(_emailSettings.Sender);
            this._errorService = errorService;
        }


        public async Task<bool> SendEmail(string receiver, string subject, string message)
        {
            MailAddress mailTo = new MailAddress(receiver);
            MailMessage mailMessage = new MailMessage(_mailFrom, mailTo)
            {
                IsBodyHtml = true, 
                Body = message, 
                Subject = subject
            };

            try
            {
                await _client.SendMailAsync(mailMessage);
            }catch(Exception e)
            {
                var errorObj = new ErrorLog
                {
                    Date = DateTime.Now,
                    ExtraData = "Email could not be sent to "+receiver,
                    Message = e.Message
                };
                await _errorService.AddErrorAsync(errorObj);
                return false;
            }

            return true;
        }
    }
}
