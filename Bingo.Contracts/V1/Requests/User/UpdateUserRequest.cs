using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.User
{
    public class UpdateUserRequest
    {
        #nullable enable
        public string? FirstName { get; set; }

        #nullable enable
        public string? LastName { get; set; }

        #nullable enable
        public string? PhoneNumber { get; set; }

        #nullable enable
        public string? Description { get; set; }
    }
}
