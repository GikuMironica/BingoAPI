using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Announcement
{
    public class GetAnnouncementsRequest
    {
        [Required(ErrorMessage = "The post id is required. You can get all announcements belonging to one post only.")]
        public int PostId { get; set; }
    }
}
