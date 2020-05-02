using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class ResetPasswordRequest
    {
        public string email { get; set; }

        public string token { get; set; }
    }
}
