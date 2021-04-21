using System;

namespace Bingo.Contracts.V1.Responses.Profile
{
    public class ProfileResponse
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Description { get; set; }

        public Double? Rating { get; set; }
    }
}
