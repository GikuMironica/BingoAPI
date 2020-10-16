using BingoAPI.Options;
using Microsoft.Extensions.Options;
namespace BingoAPI.Services
{
    /// <summary>
    /// This class, at application start, parses all JSON files related to email templates, and interpolates them with every language that is defined in EmailOptions class.
    /// Thereafter, these templates are used by IEmailFormatter service, that interpolates it again and adds missing links or tokens etc.
    /// Why? For performance reasons. This interpolation is done once at startup, then the templates are available in every language in memory. 
    /// </summary>
    public class FormattedEmailSingleton
    {
        private readonly EmailOptions _emailOptions;

        public RegisterEmailTemplate RegisterTemplate { get; set; } = new RegisterEmailTemplate();
        public ForgotPasswordTemplate ForgotPasswordTemplate { get; set; } = new ForgotPasswordTemplate();

        public FormattedEmailSingleton(IOptions<EmailOptions> emailOptions)
        {
            this._emailOptions = emailOptions.Value;
            //this.RegisterTemplate = new RegisterEmailTemplate();
           // this.ForgotPasswordTemplate = new ForgotPasswordTemplate();
            FormatAll();
        }

        private void FormatAll()
        {
            // Format the register email templates first for all languages---------------------------------------------------------------------------------------
            var jsonEmailContent = _emailOptions.RegisterConfirmation.EmailHtmlTemplate;
            RegisterTemplate.English = jsonEmailContent
                .Replace("{EmailRegistered}", _emailOptions.RegisterConfirmation.Languages.English.EmailRegistered)
                .Replace("{MessagePart2}", _emailOptions.RegisterConfirmation.Languages.English.MessagePart2)
                .Replace("{ConfirmationBtnText}", _emailOptions.RegisterConfirmation.Languages.English.ConfirmationBtnText)
                .Replace("{MessagePart4}", _emailOptions.RegisterConfirmation.Languages.English.MessagePart4)
                .Replace("{Warning}", _emailOptions.RegisterConfirmation.Languages.English.Warning)
                .Replace("{Footer}", _emailOptions.RegisterConfirmation.Languages.English.Footer);

            // Format the forgot password email templates  for all languages------------------------------------------------------------------------------------------
            jsonEmailContent = _emailOptions.ForgotPasswordConfirmation.EmailHtmlTemplate;
            ForgotPasswordTemplate.English = jsonEmailContent
                .Replace("{ResetPasswordTitle}", _emailOptions.ForgotPasswordConfirmation.Languages.English.ResetPasswordTitle)
                .Replace("{ResetPasswordMessage}", _emailOptions.ForgotPasswordConfirmation.Languages.English.ResetPasswordMessage)
                .Replace("{GeneratePasswordBtn}", _emailOptions.ForgotPasswordConfirmation.Languages.English.GeneratePasswordBtn)
                .Replace("{Warning}", _emailOptions.ForgotPasswordConfirmation.Languages.English.Warning)
                .Replace("{Footer}", _emailOptions.ForgotPasswordConfirmation.Languages.English.Footer);
        }
    }
    public class RegisterEmailTemplate
    {
        public string English { get; set; }
    }

    public class ForgotPasswordTemplate
    {
        public string English { get; set; }
    }

}
