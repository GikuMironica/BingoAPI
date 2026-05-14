using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BingoAPI.Domain;

namespace BingoAPI.Options
{
    public class EventTypes
    {
        public EventTypes()
        {

        }
        public List<EventConfiguration> Types { get; set; }
    }

}
