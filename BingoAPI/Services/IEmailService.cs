using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IEmailService
    {
        public Task<bool> SendEmail(string receiver, string subject, string message);

    }
}
