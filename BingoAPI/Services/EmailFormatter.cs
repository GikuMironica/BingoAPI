using BingoAPI.Domain;
using BingoAPI.Options;
using Microsoft.Extensions.Options;
using System;

namespace BingoAPI.Services
{
    /// <summary>
    /// This class fetches the email HTML templates preloaded in the desired language from the FormatEmailSingleton class.
    /// Therefore, these templates are interpolated, missing "links", "email subject" and other stuff is added. Finally, all components for an email are here assembled,
    /// and returned.
    /// </summary>
    public class EmailFormatter : IEmailFormatter
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly EmailOptions _emailOptions;
        private readonly FormattedEmailSingleton _formattedEmail;
        
        public EmailFormatter(IOptions<EmailOptions> emailOptions, FormattedEmailSingleton formattedEmail)
        {
            this._emailOptions = emailOptions.Value;
            this._formattedEmail = formattedEmail;
        }
        public EmailFormatResult FormatRegisterConfirmation(string emailAddress, string confirmationLink, String? language = null)
        {            
            var formattedEmail = "";
            switch (language)
            {
                case null:
                    formattedEmail = _formattedEmail.RegisterHtmlTemplate.English;
                    break;
                case "":
                    formattedEmail = _formattedEmail.RegisterHtmlTemplate.English;
                    break;
                case "en":
                    formattedEmail = _formattedEmail.RegisterHtmlTemplate.English;
                    break;
                case "de":
                    formattedEmail = _formattedEmail.RegisterHtmlTemplate.German;
                    break;
                default:
                    formattedEmail = _formattedEmail.RegisterHtmlTemplate.English;
                    break;
            }                        

            var finalEmail = formattedEmail
                .Replace("{ConfirmationLink}", confirmationLink);

            return new EmailFormatResult
            {
                EmailContent = finalEmail,
                EmailSubject = _emailOptions.RegisterConfirmation.Languages.English.Subject
            };
        }

        public EmailFormatResult FormatForgotPassword(string generateLink, String? language = null)
        {
            var formattedEmail = "";
            switch (language)
            {
                case null:
                    formattedEmail = _formattedEmail.ForgotPasswordHtmlTemplate.English;
                    break;
                case "":
                    formattedEmail = _formattedEmail.ForgotPasswordHtmlTemplate.English;
                    break;
                case "en":
                    formattedEmail = _formattedEmail.ForgotPasswordHtmlTemplate.English;
                    break;
                case "de":
                    formattedEmail = _formattedEmail.ForgotPasswordHtmlTemplate.German;
                    break;
                default:
                    formattedEmail = _formattedEmail.ForgotPasswordHtmlTemplate.English;
                    break;
            }

            var finalEmail = formattedEmail
                .Replace("{GenerateLink}", generateLink);

            return new EmailFormatResult
            {
                EmailContent = finalEmail,
                EmailSubject = _emailOptions.ForgotPasswordConfirmation.Languages.English.Subject
            };
        }

        public EmailFormatResult FormatResetPassword(String? language = null)
        {
            var formattedEmail = "";
            switch (language)
            {
                case null:
                    formattedEmail = _formattedEmail.ResetPasswordHtmlTemplate.English;
                    break;
                case "":
                    formattedEmail = _formattedEmail.ResetPasswordHtmlTemplate.English;
                    break;
                case "en":
                    formattedEmail = _formattedEmail.ResetPasswordHtmlTemplate.English;
                    break;
                case "de":
                    formattedEmail = _formattedEmail.ResetPasswordHtmlTemplate.German;
                    break;
                default:
                    formattedEmail = _formattedEmail.ResetPasswordHtmlTemplate.English;
                    break;
            }
            
            return new EmailFormatResult
            {
                EmailContent = formattedEmail,
                EmailSubject = _emailOptions.ResetPasswordConfirmation.Languages.English.Subject
            };
        }
    }
}
