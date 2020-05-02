using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; }
        public string OldPass { get; set; }

        public string NewPasword { get; set; }
    }
}
