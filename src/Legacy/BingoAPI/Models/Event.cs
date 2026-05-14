using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    [Table("Events")]
    public abstract class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public double? EntrancePrice { get; set; }

        public int? Currency { get; set; }

        public string? Title { get; set; }

        public string Description{ get; set; }
        #nullable enable
        public string? Requirements { get; set; }

        public Post? Post { get; set; }

        public int PostId { get; set; }

        public virtual int GetSlotsIfAny()
        {
            return 0;
        }

    }

   
}
