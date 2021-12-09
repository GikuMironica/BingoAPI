namespace BingoAPI.Options
{
    public class EmailOptions
    {
        public string Footer { get; set; }  
        public RegisterConfirmation RegisterConfirmation { get; set; }
        public ForgotPasswordConfirmation ForgotPasswordConfirmation { get; set; }
        public ResetPasswordConfirmation ResetPasswordConfirmation { get; set; }
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
    
    public class ResetPasswordConfirmation
    {
        public string EmailHtmlTemplate { get; set; }
        public ResetPasswordLanguages Languages { get; set; }

    }

    // LANGUAGES --------------------------------------------------------------------------------------------------------------------------------------------------

    public class RegisterConfirmationLanguages
    {
        public EnglishRC English { get; set; }
        public GermanRC German { get; set; }

        /// <summary>
        ///  ENGLISH TEMPLATE
        /// </summary>
        public class EnglishRC
        {
            public string Subject { get; set; }
            public string EmailRegistered { get; set; }
            public string MessagePart2 { get; set; }
            public string ConfirmationBtnText { get; set; }
            public string MessagePart4 { get; set; }
            public string Warning { get; set; }
        }
        /// <summary>
        ///  GERMAN TEMPLATE
        /// </summary>
        public class GermanRC
        {
            public string Subject { get; set; }
            public string EmailRegistered { get; set; }
            public string MessagePart2 { get; set; }
            public string ConfirmationBtnText { get; set; }
            public string MessagePart4 { get; set; }
            public string Warning { get; set; }
        }
    }

    public class ForgotPasswordLanguages
    {
        public EnglishFC English { get; set; }
        public GermanFC German { get; set; }

        /// <summary>
        ///  ENGLISH TEMPLATE
        /// </summary>
        public class EnglishFC
        {
            public string Subject { get; set; }
            public string ResetPasswordTitle { get; set; }
            public string ResetPasswordMessage { get; set; }
            public string GeneratePasswordBtn { get; set; }
            public string Warning { get; set; }
        }
        /// <summary>
        ///  GERMAN TEMPLATE
        /// </summary>
        public class GermanFC
        {
            public string Subject { get; set; }
            public string ResetPasswordTitle { get; set; }
            public string ResetPasswordMessage { get; set; }
            public string GeneratePasswordBtn { get; set; }
            public string Warning { get; set; }
        }
    }

    public class ResetPasswordLanguages
    {
        public EnglishPR English { get; set; }
        public GermanPR German { get; set; }

        /// <summary>
        ///  ENGLISH TEMPLATE
        /// </summary>
        public class EnglishPR
        {
            public string Subject { get; set; }
            public string PasswordResetTitle { get; set; }
            public string UsageText { get; set; }
        }
        /// <summary>
        ///  GERMAN TEMPLATE
        /// </summary>
        public class GermanPR
        {
            public string Subject { get; set; }
            public string PasswordResetTitle { get; set; }
            public string UsageText { get; set; }
        }
    }


}
