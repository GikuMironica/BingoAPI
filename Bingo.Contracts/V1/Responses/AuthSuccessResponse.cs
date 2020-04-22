using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses
{
    public class AuthSuccessResponse
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }
}
