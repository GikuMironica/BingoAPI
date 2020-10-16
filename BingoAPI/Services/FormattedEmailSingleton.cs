using BingoAPI.Options;
using Microsoft.Extensions.Options;
namespace BingoAPI.Services
{
    public class FormattedEmailSingleton
    {
        private readonly EmailOptions _emailOptions;

        public RegisterEmailTemplate RegisterTemplate { get; set; } = new RegisterEmailTemplate();

        public FormattedEmailSingleton(IOptions<EmailOptions> emailOptions)
        {
            this._emailOptions = emailOptions.Value;
            FormatAll();
        }

        private void FormatAll()
        {
            var emailContent = _emailOptions.RegisterConfirmation.EmailHTMLTemplate;
            RegisterTemplate.English = emailContent
                        .Replace("{EmailRegistered}", _emailOptions.RegisterConfirmation.Languages.en.EmailRegistered)
                        .Replace("{MessagePart2}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart2)
                        .Replace("{ConfirmationBtnText}", _emailOptions.RegisterConfirmation.Languages.en.ConfirmationBtnText)
                        .Replace("{MessagePart4}", _emailOptions.RegisterConfirmation.Languages.en.MessagePart4)
                        .Replace("{Warning}", _emailOptions.RegisterConfirmation.Languages.en.Warning)
                        .Replace("{Footer}", _emailOptions.RegisterConfirmation.Languages.en.Footer);
        }
    }
    public class RegisterEmailTemplate
    {
        public string English { get; set; }
    }
}
