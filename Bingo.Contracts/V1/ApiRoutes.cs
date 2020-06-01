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

        public static class Identity
        {
            public const string Login = Base + "/identity/login";

            public const string Register = Base + "/identity/register";

            public const string Refresh = Base + "/identity/refresh";

            public const string ConfirmEmail = Base + "/identity/confirmemail";

            public const string FacebookAuth = Base + "/identity/auth/fb";

            public const string ForgotPassword = Base + "/identity/forgotpassword";

            public const string ResetPassword = Base + "/identity/resetpassword";

            public const string ChangePassword = Base + "/identity/changepassword";
        }

        public static class Users
        {
            public const string GetAll = Base + "/users";

            public const string Update = Base + "/users/{userId}";

            public const string Delete = Base + "/users/{userId}";

            public const string Get = Base + "/users/{userId}";

        }

        public static class Posts
        {
            public const string GetAll = Base + "/posts";

            public const string Get = Base + "/posts/{postId}";

            public const string Create = Base + "/posts";

            public const string Update = Base + "/posts/{postId}";

            public const string Delete = Base + "/posts/{postId}";
        }

        public static class AttendedEvents
        {
            public const string Attend = Base + "/attend/{postId}";
            public const string UnAttend = Base + "/unattend/{postId}";
            public const string GetActiveAttendedPosts = Base + "/attendedactive";
            public const string GetInActiveAttendedPosts = Base + "/attendedinactive";
        }
    }
}
