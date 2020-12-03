using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface INotificationService
    {
        public Task NotifyAttendEventRequestAcceptedAsync(List<string> usersId, string eventTitle, int postId);
        public Task NotifyHostNewParticipationRequestAsync(List<string> usersId, string fullname, int postId);
        public Task NotifyParticipantsEventUpdatedAsync(List<string> usersId, string eventTitle, int postId);
        public Task NotifyParticipantsEventDeletedAsync(List<string> usersId, string eventTitle);
        public Task NotifyParticipantsNewAnnouncementAsync(List<string> usersId, string eventTitle, int postId);
    }
}
