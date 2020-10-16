using BingoAPI.Domain;
using BingoAPI.Options;
using Microsoft.Extensions.Options;
using System;

namespace BingoAPI.Services
{
    public class EmailFormatter : IEmailFormatter
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly EmailOptions _emailOptions;
        private readonly FormattedEmailSingleton _formattedEmail;

        public string EmailSubject { get; set; }

        public EmailFormatter(IOptions<EmailOptions> emailOptions, FormattedEmailSingleton formattedEmail)
        {
            this._emailOptions = emailOptions.Value;
            EmailSubject = emailOptions.Value.RegisterConfirmation.Languages.en.Subject;
            this._formattedEmail = formattedEmail;
        }
        public EmailFormatResult FormatRegisterConfirmation(string emailAddress, string confirmationLink, String? language = null)
        {            
            var formattedEmail = "";
            switch (language)
            {
                case null:
                    formattedEmail = _formattedEmail.RegisterTemplate.English;
                    break;
                case "en":
                    formattedEmail = _formattedEmail.RegisterTemplate.English;
                    break;
                case "":
                    formattedEmail = _formattedEmail.RegisterTemplate.English;
                    break;
            }                        

            var finalEmail = formattedEmail
                .Replace("{ConfirmationLink}", confirmationLink);

            return new EmailFormatResult
            {
                EmailContent = finalEmail,
                EmailSubject = this.EmailSubject
            };
        }
    }
}
