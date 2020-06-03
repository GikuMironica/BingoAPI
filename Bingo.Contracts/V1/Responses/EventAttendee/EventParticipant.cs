using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.EventAttendee
{
    public class EventParticipant
    {
        public string? Picture { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
