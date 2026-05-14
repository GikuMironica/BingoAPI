using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class MyServicesHttpClient
    {
        public HttpClient HttpClient { get; set; }
        public MyServicesHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }
    }
}
