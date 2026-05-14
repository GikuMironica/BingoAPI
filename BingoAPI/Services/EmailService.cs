using BingoAPI.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _client;
        private readonly MailAddress _mailFrom;
        private readonly ApplicationEmailSettings _emailSettings;

        public EmailService(IOptions<ApplicationEmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _client = new SmtpClient(_emailSettings.SmtpClient);
            _client.Credentials = new System.Net.NetworkCredential(_emailSettings.EmailAddress, _emailSettings.Password);
            _client.Port = _emailSettings.Port;
            _client.EnableSsl = _emailSettings.SSL;
            _mailFrom = new MailAddress(_emailSettings.Sender);
            _logger = logger;
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
                _logger.LogError(e, "Email could not be sent to {Receiver}", receiver);
                return false;
            }

            return true;
        }
    }
}
