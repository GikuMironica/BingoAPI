using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.User
{
    public class UserResponse
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePicture { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public Int64 RegistrationTimeStamp { get; set; }
    }
}
