using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IEmailFormatter
    {
        /// <summary>
        /// This method selects the desired language HTML template for Email Confirmation messages
        /// and adds the missing details to it. Like tokens that are generated at runtime
        /// </summary>
        /// <param name="emailAddress">Email address of the user that just registered</param>
        /// <param name="confirmationLink">Confirmation link generated</param>
        /// <param name="language">User's device language</param>
        /// <returns>All components for sending an HTML content email</returns>
        public EmailFormatResult FormatRegisterConfirmation(string emailAddress, string confirmationLink, String? language = null);

        /// <summary>
        /// This email selects the desired HTML template for Forgot Password requests.
        /// It adds to the template a confirmation link that is generated at runtime.
        /// </summary>
        /// <param name="generateLink">Confirmation Link that will generate a new password for the user</param>
        /// <param name="language">User's device language</param>
        /// <returns>All components required for sending and HTML content email</returns>
        public EmailFormatResult FormatForgotPassword(string generateLink, String? language = null);

        /// <summary>
        /// This method selects a specific HTML template based on user's device language.
        /// It will display the reset password html view in the email.
        /// </summary>
        /// <param name="language">user's device language</param>
        /// <returns></returns>
        public EmailFormatResult FormatResetPassword(String? language = null);
    }
}
