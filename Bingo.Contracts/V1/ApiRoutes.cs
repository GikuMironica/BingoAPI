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

            public const string FacebookAuth = Base + "/identity/auth/fb";
        }
    }
}
