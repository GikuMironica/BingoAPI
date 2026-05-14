using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.EventAttendee
{
    public class EventParticipantData
    {
        public int AttendeesNumber { get; set; }
        public List<EventParticipant> Attendees { get; set; }
    }
}
