using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace BingoAPI.Models
{
    [Table("EventLocation")]
    public class EventLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string? EntityName { get; set; }

        public string? City { get; set; }

        public string? Region { get; set; }

        public string? Address { get; set; }

        public string? Country { get; set; }

        [Column(TypeName = "geography (point)")]
        public Point Location { get; set; }

        public Post Post { get; set; }

        public int PostId { get; set; }
    }
}
