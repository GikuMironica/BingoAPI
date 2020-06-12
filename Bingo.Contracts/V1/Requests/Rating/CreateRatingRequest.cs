using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Rating
{
    public class CreateRatingRequest
    {
        [Required]
        [Range(1,5)]
        public int Rate { get; set; }

        [Required]
        public string UserId { get; set; }
        
        [Required]
        public int PostId { get; set; }

        public String? Feedback { get; set; }
    }
}
