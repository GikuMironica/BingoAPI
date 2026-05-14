using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class GatewayCheckoutRequest
    {
        public string PaymentNonce { get; set; }

        public string DeviceData { get; set; }
        
        public string FeatureId { get; set; }

        public string UserId { get; set; }
        
        public string UserEmail { get; set; }
    }
}
