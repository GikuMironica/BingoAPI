using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Options
{
    public class ApplicationEmailSettings
    {
        public string EmailAddress { get; set; }

        public string Sender { get; set; }

        public string Password { get; set; }

        public string SmtpClient { get; set; }

        public int Port { get; set; }

        public bool SSL { get; set; }
    }
}
