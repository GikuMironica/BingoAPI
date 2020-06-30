
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI.Domain;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomMapper
{
    public class DomainToResponseMapper : IDomainToResponseMapper
    {
        public Posts MapPostForGetAllPostsReponse(Post post, EventTypes eventTypes)
        {
            string eventType = post.Event.GetType().Name.ToString();

            var eventTypeNumber = eventTypes.Types 
            .Where(y => y.Type == eventType)
            .Select(x => x.Id)
            .FirstOrDefault();

            return new Posts
            {
                PostId = post.Id,
                Address = post.Location.Address,
                Thumbnail = post.Pictures.FirstOrDefault(),
                PostType = eventTypeNumber,
                Title = post.Event.Title,
                Logitude = post.Location.Location.X,
                Latitude = post.Location.Location.Y,
                EntracePrice = post.Event.EntrancePrice ?? 0,
                Frequency = post.Repeatable.Frequency,
                StartTime = post.EventTime,
                EndTime = post.EndTime.GetValueOrDefault(DateTimeOffset.UtcNow.ToUnixTimeSeconds()+21600),
                PostTime = post.PostTime,
                RepteatableEnabled = post.Repeatable.Enabled,
                VouchersEnabled = post.Voucher.Enabled
            };
        }
    }
}
