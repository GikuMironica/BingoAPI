using Bingo.Contracts.V1.Responses.AttendedEvent;
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI.Domain;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomMapper
{
    public interface IDomainToResponseMapper
    {
        public Posts MapPostForGetAllPostsReponse(Post post, EventTypes eventTypes);
        public MiniPostForAnnouncements MapMiniPostForAnnouncementsList(Post post, EventTypes eventTypes);
    }
}
