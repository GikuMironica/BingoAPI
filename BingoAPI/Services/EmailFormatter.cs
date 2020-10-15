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
        public string EmailSubject { get; set; }

        public EmailFormatter(IOptions<EmailOptions> emailOptions)
        {
            this._emailOptions = emailOptions.Value;
            EmailSubject = emailOptions.Value.RegisterConfirmation.Languages.en.Subject;
        }
        public EmailFormatResult FormatRegisterConfirmation(string EmailAdress, string ConfirmationLink, String? Language = null)
        {
            var EmailContent = _emailOptions.RegisterConfirmation.EmailHTMLTemplate;
            var FormattedEmail = "";
            switch (Language)
            {
                case null: 
                        FormattedEmail = EmailContent
                        .Replace("{EmailRegistered}", _emailOptions.RegisterConfirmation.Languages.en.EmailRegistered)
                        .Replace("{MessagePart2}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart2)
                        .Replace("{ConfirmationBtnText}", _emailOptions.RegisterConfirmation.Languages.en.ConfirmationBtnText)
                        .Replace("{MessagePart4}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart4)
                        .Replace("{Warning}", _emailOptions.RegisterConfirmation.Languages.en.Warning)
                        .Replace("{Footer}", _emailOptions.RegisterConfirmation.Languages.en.Footer);
                    break;
                case "en":
                        FormattedEmail = EmailContent
                        .Replace("{EmailRegistered}", _emailOptions.RegisterConfirmation.Languages.en.EmailRegistered)
                        .Replace("{MessagePart2}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart2)
                        .Replace("{ConfirmationBtnText}", _emailOptions.RegisterConfirmation.Languages.en.ConfirmationBtnText)
                        .Replace("{MessagePart4}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart4)
                        .Replace("{Warning}", _emailOptions.RegisterConfirmation.Languages.en.Warning)
                        .Replace("{Footer}", _emailOptions.RegisterConfirmation.Languages.en.Footer);
                    break;
                case "":
                    FormattedEmail = EmailContent
                    .Replace("{EmailRegistered}", _emailOptions.RegisterConfirmation.Languages.en.EmailRegistered)
                    .Replace("{MessagePart2}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart2)
                    .Replace("{ConfirmationBtnText}", _emailOptions.RegisterConfirmation.Languages.en.ConfirmationBtnText)
                    .Replace("{MessagePart4}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart4)
                    .Replace("{Warning}", _emailOptions.RegisterConfirmation.Languages.en.Warning)
                    .Replace("{Footer}", _emailOptions.RegisterConfirmation.Languages.en.Footer);
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
