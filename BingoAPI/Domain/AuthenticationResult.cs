using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class AuthenticationResult
    {
        public string Token { get; set; }

        public bool Success { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public string RefreshToken { get; set; }

        public string UserId { get; set; }

        public FailReason FailReason { get; set; }
    }

    public enum FailReason
    {
        EmailNotConfirmed = 0,
        TooManyInvalidAttempts = 1,
        InvalidPassword = 2
    }
}
