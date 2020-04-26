using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class RefreshTokenRequest
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }
}
