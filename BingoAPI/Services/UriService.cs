using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.User;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class UriService : IUriService
    {

        private readonly string _baseUri;

        public UriService(string baseUri)
        {
            _baseUri = baseUri;
        }
               

        public Uri GetPostUri(string postId)
        {
            return new Uri(_baseUri + ApiRoutes.Posts.Get.Replace("{postId}", postId));
        }

        public Uri GetAllUsersUri(PaginationQuery pagination = null)
        {
            var uri = new Uri(_baseUri);

            if (pagination == null)
            {
                return uri;
            }

            var modifiedUri = QueryHelpers.AddQueryString(_baseUri, "pageNumber", pagination.PageNumber.ToString());
            modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", pagination.PageSize.ToString());

            return new Uri(modifiedUri);
        }

        public Uri GetRatingUri(string ratingId)
        {
            return new Uri(_baseUri + ApiRoutes.Ratings.Get.Replace("{ratingId}", ratingId));
        }

        public Uri GetReportUri(string reportId)
        {
            return new Uri(_baseUri + ApiRoutes.Reports.Get.Replace("{reportId}", reportId));
        }

        public Uri GetAnnouncementUri(string announcementId)
        {
            return new Uri(_baseUri + ApiRoutes.Announcements.Get.Replace("{announcementId}", announcementId));
        }

        public Uri GetUserReportUri(string reportId)
        {
            return new Uri(_baseUri + ApiRoutes.UserReports.Get.Replace("{reportId}", reportId));
        }
    }
}
