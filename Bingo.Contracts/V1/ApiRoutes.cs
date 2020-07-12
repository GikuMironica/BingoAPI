using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1
{
    public class ApiRoutes
    {

        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version;

        public static class Error
        {
            public const string ErrorRoute = "/Error";
        }
        public static class Identity
        {
            public const string Login = Base + "/identity/login";

            public const string Register = Base + "/identity/register";

            public const string Refresh = Base + "/identity/refresh";

            public const string ConfirmEmail = Base + "/identity/confirmemail";

            public const string AdminConfirmEmail = Base + "/identity/confirmemail";

            public const string FacebookAuth = Base + "/identity/auth/fb";

            public const string ForgotPassword = Base + "/identity/forgotpassword";

            public const string ResetPassword = Base + "/identity/resetpassword";

            public const string ChangePassword = Base + "/identity/changepassword";

            public const string AddPassword = Base + "/identity/addpassword";
        }

        public static class Users
        {
            public const string GetAll = Base + "/users";

            public const string Update = Base + "/users/{userId}";

            public const string UpdateProfilePicture = Base + "/users/updateprofilepic/{userId}";

            public const string Delete = Base + "/users/{userId}";

            public const string DeletePicture = Base + "/users/deleteprofilepic/{userId}";

            public const string Get = Base + "/users/{userId}";

        }

        public static class Posts
        {
            public const string GetAll = Base + "/posts";

            public const string GetAllActive = Base + "/posts/myactive";

            public const string GetAllInactive = Base + "/posts/myinactive";

            public const string Get = Base + "/posts/{postId}";

            public const string Create = Base + "/posts";

            public const string Update = Base + "/posts/{postId}";

            public const string Delete = Base + "/posts/{postId}";

            public const string DisablePost = Base + "/posts/disable";
        }

        public static class AttendedEvents
        {
            public const string Attend = Base + "/attend/{postId}";
            public const string UnAttend = Base + "/unattend/{postId}";
            public const string GetActiveAttendedPosts = Base + "/attendedactive";
            public const string GetInactiveAttendedPosts = Base + "/attendedinactive";
        }

        public static class EventAttendees
        {
            public const string Accept = Base + "/acceptattendee";
            public const string Reject = Base + "/rejectattendee";
            public const string FetchAll = Base + "/fetchallattendee";
            public const string FetchAccepted = Base + "/fetchallaccepted";
            public const string FetchPending = Base + "/fetchallpending";
        }

        public static class Announcements
        {
            public const string GetAll = Base + "/announcements/postId/{postId}";
            public const string Get = Base + "/announcements/{announcementId}";
            public const string Create = Base + "/announcements";
            public const string Update = Base + "/announcements/{announcementId}";
            public const string Delete = Base + "/announcements/{announcementId}";
        }

        public static class Ratings
        {
            public const string GetAll = Base + "/ratings/userId/{userId}";
            public const string Get = Base + "/ratings/{ratingId}";
            public const string Create = Base + "/ratings";
            public const string Delete = Base + "/ratings/{ratingId}";
        }

        public static class Reports
        {
            public const string GetAll = Base + "/reports/userId/{userId}";
            public const string Get = Base + "/reports/{reportId}";
            public const string Create = Base + "/reports";
            public const string Delete = Base + "/reports/{reportId}";
            public const string DeleteAll = Base + "/reports/userId/{userId}";
        }

        public static class UserReports
        {
            public const string GetAll = Base + "/userreports/userId/{userId}";
            public const string Get = Base + "/userreports/{reportId}";
            public const string Create = Base + "/userreports";
            public const string Delete = Base + "/userreports/{reportId}";
        }

        public static class Profile
        {
            public const string Get = Base + "/profile/{userId}";
        }
        public static class Tag
        {
            public const string GetAll = Base + "/tag/{TagName}";
        }
    }
}
