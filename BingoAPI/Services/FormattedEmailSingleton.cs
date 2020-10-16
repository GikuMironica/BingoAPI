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

        public RegisterEmailTemplate RegisterHtmlTemplate { get; set; }
        public ForgotPasswordTemplate ForgotPasswordHtmlTemplate { get; set; }
        public ResetPasswordTemplate ResetPasswordHtmlTemplate { get; set; }  

        public FormattedEmailSingleton(IOptions<EmailOptions> emailOptions)
        {
            this._emailOptions = emailOptions.Value;
            this.RegisterHtmlTemplate = new RegisterEmailTemplate();
            this.ForgotPasswordHtmlTemplate = new ForgotPasswordTemplate();
            this.ResetPasswordHtmlTemplate = new ResetPasswordTemplate();
            FormatAll();
        }

        private void FormatAll()
        {
            // Format the register email templates first for all languages---------------------------------------------------------------------------------------
            var jsonEmailContent = _emailOptions.RegisterConfirmation.EmailHtmlTemplate;
            // alias
            var inputRegEng = _emailOptions.RegisterConfirmation.Languages.English;

            RegisterHtmlTemplate.English = jsonEmailContent
                .Replace("{EmailRegistered}", inputRegEng.EmailRegistered)
                .Replace("{MessagePart2}", inputRegEng.MessagePart2)
                .Replace("{ConfirmationBtnText}", inputRegEng.ConfirmationBtnText)
                .Replace("{MessagePart4}", inputRegEng.MessagePart4)
                .Replace("{Warning}", inputRegEng.Warning)
                .Replace("{Footer}", _emailOptions.Footer);

            // Format the forgot password email templates  for all languages------------------------------------------------------------------------------------------
            jsonEmailContent = _emailOptions.ForgotPasswordConfirmation.EmailHtmlTemplate;
            // alias
            var inputForEng = _emailOptions.ForgotPasswordConfirmation.Languages.English;

            ForgotPasswordHtmlTemplate.English = jsonEmailContent
                .Replace("{ForgotPasswordTitle}", inputForEng.ResetPasswordTitle)
                .Replace("{ResetPasswordMessage}", inputForEng.ResetPasswordMessage)
                .Replace("{GeneratePasswordBtn}", inputForEng.GeneratePasswordBtn)
                .Replace("{Warning}", inputForEng.Warning)
                .Replace("{Footer}", _emailOptions.Footer);

            // Password reset email template for all langs ------------------------------------------------------------------------------------------------------------
            jsonEmailContent = _emailOptions.ResetPasswordConfirmation.EmailHtmlTemplate;
            //alias
            var inputResEng = _emailOptions.ResetPasswordConfirmation.Languages.English;
            ResetPasswordHtmlTemplate.English = jsonEmailContent
                .Replace("{PasswordResetTitle}", inputResEng.PasswordResetTitle)
                .Replace("{UsageText}", inputResEng.UsageText)
                .Replace("{Footer}", _emailOptions.Footer);
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

    public class ResetPasswordTemplate
    {
        public string English { get; set; }
    }

}
