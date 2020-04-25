using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class UserFacebookAuthRequest
    {
        public string AccessToken { get; set; }
    }
}
