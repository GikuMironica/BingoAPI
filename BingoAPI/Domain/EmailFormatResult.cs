using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    /// <summary>
    /// This class contains two components for sending an email with HTML body.
    /// The content, and the subject.
    /// </summary>
    public class EmailFormatResult
    {
        public string EmailContent { get; set; }
        public string EmailSubject { get; set; }
    }
}
