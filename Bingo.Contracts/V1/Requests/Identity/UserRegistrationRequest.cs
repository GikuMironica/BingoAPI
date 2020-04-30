 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class UserRegistrationRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
