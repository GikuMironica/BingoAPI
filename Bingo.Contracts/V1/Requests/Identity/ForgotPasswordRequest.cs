using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }
}
