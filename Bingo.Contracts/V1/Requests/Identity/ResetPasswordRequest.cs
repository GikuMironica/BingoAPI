using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class ResetPasswordRequest
    {
        [Required]
        public string email { get; set; }

        [Required]
        public string token { get; set; }
    }
}
