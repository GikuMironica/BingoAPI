using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    [Table("Events")]
    public class HouseParty : Event
    {
        public int? Slots { get; set; }

        public override int GetSlotsIfAny()
        {
            if (Slots.HasValue)
                return Slots.Value;

            return 0;
        }
    }
}
