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

        public Int64 PostTime { get; set; }

        public Int64 EventTime { get; set; }

        public Int64? EndTime { get; set; }

        public int ActiveFlag { get; set; }

        [Required]
        public EventLocation Location { get; set; }

        [Required]
        public AppUser User { get; set; }

        public string UserId { get; set; }
        
        public Event Event { get; set; }

        #nullable enable
        public List<string>? Pictures { get; set; }

        // todo
        public int Repeatable { get; set; }

#nullable enable
        public virtual List<PostTags>? Tags { get; set; }

        //todo
        public List<Participation> Participators { get; set; }

        //todo
        public List<Announcement> Announcements { get; set; }

        //todo
        public List<Report> Reports { get; set; }
    }
    
}
