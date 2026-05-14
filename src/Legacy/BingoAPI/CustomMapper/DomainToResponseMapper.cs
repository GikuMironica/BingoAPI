using Bingo.Contracts.V1.Responses.AttendedEvent;
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI.Models;
using System;
using System.Linq;
using BingoAPI.Options;

namespace BingoAPI.CustomMapper
{
    public class DomainToResponseMapper : IDomainToResponseMapper
    {
        public Posts MapPostForGetAllPostsResponse(Post post, EventTypes eventTypes)
        {
            string eventType = post.Event.GetType().Name;

            var eventTypeNumber = eventTypes.Types 
            .Where(y => y.Type == eventType)
            .Select(x => x.Id)
            .FirstOrDefault();

            return new Posts
            {
                PostId = post.Id,
                Address = post.Location.Address,
                Thumbnail = post.Pictures?.FirstOrDefault()?.Url,
                PostType = eventTypeNumber,
                Title = post.Event.Title,
                Longitude = post.Location.Location.X,
                Latitude = post.Location.Location.Y,
                EntracePrice = post.Event.EntrancePrice ?? 0,
                Frequency = post.Repeatable.Frequency,
                StartTime = post.EventTime,
                EndTime = post.EndTime.GetValueOrDefault(DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds()+21600),
                PostTime = post.PostTime,
                RepteatableEnabled = post.Repeatable.Enabled,
            };
        }

        public MiniPostForAnnouncements MapMiniPostForAnnouncementsList(Post post, EventTypes eventTypes)
        {
            string eventType = post.Event.GetType().Name;
            var eventTypeNumber = eventTypes.Types
            .Where(y => y.Type == eventType)
            .Select(x => x.Id)
            .FirstOrDefault();

            var lastAnnouncement = post.Announcements?.OrderByDescending(a => a.Timestamp)?.FirstOrDefault();
                     

            return new MiniPostForAnnouncements
            {
                PostId = post.Id,               
                Thumbnail = post.Pictures?.FirstOrDefault()?.Url,
                PostType = eventTypeNumber,
                Title = post.Event.Title,
                LastMessage = lastAnnouncement?.Message,
                LastMessageTime = lastAnnouncement?.Timestamp               
            };
        }
    }
}
