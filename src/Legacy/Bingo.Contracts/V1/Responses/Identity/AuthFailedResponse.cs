using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Identity
{
    public class AuthFailedResponse
    {
        public int FailReason { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
