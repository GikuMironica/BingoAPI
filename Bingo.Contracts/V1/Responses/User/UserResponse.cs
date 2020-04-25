using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.User
{
    public class UserResponse
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePicture { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
    }
}
