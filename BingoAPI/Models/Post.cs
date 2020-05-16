using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public long PostTime { get; set; }

        public long EventTime { get; set; }

        [Required]
        public Location Location { get; set; }

        public int LocationId { get; set; }

        [Required]
        public AppUser User { get; set; }

        public string UserId { get; set; }
        
        public Event Event { get; set; }

        public int EventId { get; set; }

        #nullable enable
        public IEnumerable<string>? Pictures { get; set; }

        #nullable enable
        public virtual List<PostTags>? Tags { get; set; }
    }
    
}
