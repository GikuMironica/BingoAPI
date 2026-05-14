using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Rate { get; set; }

        public string UserId { get; set; }

        public AppUser User { get; set; }

        public string RaterId { get; set; }

        public AppUser Rater { get; set; }

        public int PostId { get; set; }

        public Post Post { get; set; }

        public String Feedback { get; set; }
    }
}
