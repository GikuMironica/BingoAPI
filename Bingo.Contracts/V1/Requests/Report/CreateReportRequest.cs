using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Report
{
    public class CreateReportRequest
    {
        [Required]
        [Range(0,3)]
        public int Reason { get; set; }

        /*[MinLength(10, ErrorMessage = "Minimum length of the report message is 10 char")]
        [Required]*/
        public string Message { get; set; }

        [Required]
        public int PostId { get; set; }
    }
}
