using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.User
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePicture { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }
    }
}
