namespace BingoAPI.Options
{
    public class EmailOptions
    {
        public RegisterConfirmation RegisterConfirmation { get; set; }
        public ForgotPasswordConfirmation ForgotPasswordConfirmation { get; set; }
        public ReceivePasswordConfirmation ReceivePasswordConfirmation { get; set; }
    }
        
    public class RegisterConfirmation
    {
        public string EmailHtmlTemplate { get; set; }
        public RegisterConfirmationLanguages Languages { get; set; }
    }

    public class ForgotPasswordConfirmation
    {
        public string EmailHtmlTemplate { get; set; }
        public ForgotPasswordLanguages Languages { get; set; }

        
    }

    public class ReceivePasswordConfirmation
    {
    }

    public class RegisterConfirmationLanguages
    {
        public EnglishRC English { get; set; }

        public class EnglishRC
        {
            public string Subject { get; set; }
            public string EmailRegistered { get; set; }
            public string MessagePart2 { get; set; }
            public string ConfirmationBtnText { get; set; }
            public string MessagePart4 { get; set; }
            public string Warning { get; set; }
            public string Footer { get; set; }
        }
    }

    public class ForgotPasswordLanguages
    {
        public EnglishFC English { get; set; }

        public class EnglishFC
        {
            public string Subject { get; set; }
            public string ResetPasswordTitle { get; set; }
            public string ResetPasswordMessage { get; set; }
            public string GeneratePasswordBtn { get; set; }
            public string Warning { get; set; }
            public string Footer { get; set; }
        }
    }




}
