using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.Contracts.V1
{
    public class MyServicesRoutes
    {
        private const string Url = "https://localhost:44359/";
        private const string Root = "api";
        private const string Version = "v1";
        private const string Base = Url + Root + "/" + Version;

        public static class Payment
        {
            public const string GetPaymentToken = Base + "/payments/tokens";
            public const string Checkout = Base + "/payments/checkout";
        }
    }
}
