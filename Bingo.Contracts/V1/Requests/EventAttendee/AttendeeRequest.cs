using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.EventAttendee
{
    public class AttendeeRequest
    {
        [Required]
        public string AttendeeId { get; set; }
        [Required]
        public int PostId { get; set; }
    }
}
