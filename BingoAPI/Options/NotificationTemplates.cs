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
        public EventUpdated EventUpdated { get; set; }
        public EventDeleted EventDeleted { get; set; }
        public NewAnnouncement NewAnnouncement { get; set; }
    }

    public abstract class Base
    {
        public string en { get; set; }
        public string de { get; set; }
        public string ru { get; set; }
    }

    public class Heading : Base
    {        
    }

    public class AttendEventRequestAccepted : Base
    {       
    }
    
    public class HousePartyAttendRequest : Base
    {        
    }
    public class EventUpdated : Base
    {       
    }

    public class EventDeleted : Base
    {
    }

    public class NewAnnouncement : Base
    {

    }
}
