using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    [Table("Events")]
    public class Bar : Event
    {
        public double EntracePrice { get; set; }
    }
}
