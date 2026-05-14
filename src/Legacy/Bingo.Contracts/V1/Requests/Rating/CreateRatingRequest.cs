using System;
using System.ComponentModel.DataAnnotations;

namespace Bingo.Contracts.V1.Requests.Rating
{
    public class CreateRatingRequest
    {
        [Required]
        [Range(1,5)]
        public int Rate { get; set; }
        
        [Required]
        public int PostId { get; set; }

        [MaxLength(300)]
        public String? Feedback { get; set; }
    }
}
