using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        
        public List<Picture> Pictures { get; set; }

        // todo
        public RepeatableProperty Repeatable { get; set; }

#nullable enable
        public virtual List<PostTags>? Tags { get; set; }

        
        public List<Participation> Participators { get; set; }

        
        public List<Announcement> Announcements { get; set; }

       
        public List<Report> Reports { get; set; }
        
    }
    
}
