using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public string EmailHTMLTemplate { get; set; }
        public Languages Languages { get; set; }
    }

    public class ForgotPasswordConfirmation
    {
    }

    public class ReceivePasswordConfirmation
    {
    }

    public class Languages
    {
        public En en { get; set; }
    }

    public class En
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
