using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class UserFacebookAuthRequest
    {
        [Required]
        public string AccessToken { get; set; }
    }
}
