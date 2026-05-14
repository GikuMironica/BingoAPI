using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Announcement
{
    public class CreateAnnouncementRequest
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public String Message { get; set; }
    }
}
