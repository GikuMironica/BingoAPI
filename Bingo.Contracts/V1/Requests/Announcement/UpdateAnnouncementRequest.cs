using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Announcement
{
    public class UpdateAnnouncementRequest
    {
        [Required]
        [MinLength(10)]
        public String Message { get; set; }
    }
}
