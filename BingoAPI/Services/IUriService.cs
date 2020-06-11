using Bingo.Contracts.V1.Requests.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IUriService
    {

        public Uri GetPostUri(string postId);
        public Uri GetRatingUri(string ratingId);
        public Uri GetReportUri(string reportId);
        public Uri GetUserReportUri(string reportId);
        public Uri GetAnnouncementUri(string announcementId);
        public Uri GetAllUsersUri(PaginationQuery pagination);
    }
}
