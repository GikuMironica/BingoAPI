using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class AttendedEventResult
    {
        public bool Result { get; set; }
        public bool IsHouseParty { get; set; } = false;
        public string HostId { get; set; }
    }
}
