using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Options
{
    public class NotificationTemplates
    {
        public NotificationTemplates()
        {

        }

        public Heading Heading { get; set; }
        public AttendEventRequestAccepted AttendEventRequestAccepted { get; set; }
        public HousePartyAttendRequest HousePartyAttendRequest { get; set; }
    }

    public class Heading
    {
        public string en { get; set; }
        public string ru { get; set; }
    }

    public class AttendEventRequestAccepted
    {
        public string en { get; set; }
        public string ru { get; set; }
    }
    
    public class HousePartyAttendRequest
    {
        public string en { get; set; }
        public string ru { get; set; }
    }
}
