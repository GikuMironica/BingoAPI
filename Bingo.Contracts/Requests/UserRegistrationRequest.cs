using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.Requests
{
    public class UserRegistrationRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
