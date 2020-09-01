using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.UserReport
{
    public class ReportUserRequest
    {
        [Required]
        [Range(1, 6)]
        public int Reason { get; set; }

        public string? Message { get; set; }

        [Required]
        public string ReportedUserId { get; set; }
    }
}
