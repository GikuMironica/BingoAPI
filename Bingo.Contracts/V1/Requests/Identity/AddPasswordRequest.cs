using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Identity
{
    public class AddPasswordRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
