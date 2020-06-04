using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface INotificationService
    {
        public Task NotifyAttendEventRequestAcceptedAsync(List<string> usersId, string eventTitle);
    }
}
