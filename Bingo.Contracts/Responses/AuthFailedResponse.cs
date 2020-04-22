using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.Responses
{
    public class AuthFailedResponse
    {
        public IEnumerable<string> Errors { get; set; }
    }
}
