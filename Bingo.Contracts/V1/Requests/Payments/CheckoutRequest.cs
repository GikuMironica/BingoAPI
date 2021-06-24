using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.Contracts.V1.Requests.Payments
{
    public class CheckoutRequest
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string DeviceData { get; set; }

        [Required]
        public string FeatureId { get; set; }
        
    }
}
