using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    [Table("EventsLocations")]
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        #nullable enable
        public double? Logitude { get; set; }

        #nullable enable
        public double? Latitude { get; set; }

        #nullable enable
        public string? Address { get; set; }

        #nullable enable
        public string? City { get; set; }

        #nullable enable
        public string? Region { get; set; }

        #nullable enable
        public string? Country { get; set; }

        public Post Post { get; set; }

        public int PostId { get; set; }
    }
}
