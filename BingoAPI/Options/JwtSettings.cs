using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Options
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; } = "Hopaut.Api";
        public string Audience { get; set; } = "Hopaut.Clients";
        public TimeSpan TokenLifetime { get; set; }
    }
}
