using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    public class RepeatableProperty
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Enabled { get; set; }
        public Int64 EndTime { get; set; }
        public int Frequency { get; set; }
        public Post Post { get; set; }
        public int PostId { get; set; }

    }
}
