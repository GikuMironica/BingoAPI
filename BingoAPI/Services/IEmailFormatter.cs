using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IEmailFormatter
    {
        public EmailFormatResult FormatRegisterConfirmation(string emailAddress, string ConfirmationLink, String? Language = null);
    }
}
