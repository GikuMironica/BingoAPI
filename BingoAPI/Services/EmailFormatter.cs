using BingoAPI.Domain;
using BingoAPI.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class EmailFormatter : IEmailFormatter
    {
        private readonly EmailOptions _emailOptions;
        private readonly FormattedEmailSingleton _formatedEmail;

        public string EmailSubject { get; set; }

        public EmailFormatter(IOptions<EmailOptions> emailOptions, FormattedEmailSingleton formatedEmail)
        {
            this._emailOptions = emailOptions.Value;
            EmailSubject = emailOptions.Value.RegisterConfirmation.Languages.en.Subject;
            this._formatedEmail = formatedEmail;
        }
        public EmailFormatResult FormatRegisterConfirmation(string EmailAdress, string ConfirmationLink, String? Language = null)
        {            
            var FormattedEmail = "";
            switch (Language)
            {
                case null:
                    FormattedEmail = _formatedEmail.RegisterTemplate.English;
                    break;
                case "en":
                    FormattedEmail = _formatedEmail.RegisterTemplate.English;
                    break;
                case "":
                    FormattedEmail = _formatedEmail.RegisterTemplate.English;
                    break;
            }                        

            var FinalEmail = FormattedEmail
                .Replace("{ConfirmationLink}", ConfirmationLink);

            return new EmailFormatResult
            {
                EmailContent = FinalEmail,
                EmailSubject = EmailSubject
            };
        }
    }
}
