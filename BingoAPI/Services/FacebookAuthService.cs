using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BingoAPI.ExternalLogin;
using BingoAPI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BingoAPI.Services
{
    public class FacebookAuthService : IFacebookAuthService
    {

        private const string TokenValidationUrl = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}";
        private const string UserInfoUrl = "https://graph.facebook.com/me?fields=first_name,last_name,picture.height(128),email&access_token={0}";
        private readonly FacebookAuthSettings facebookAuthSettings;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<FacebookAuthService> _logger;

        public FacebookAuthService(FacebookAuthSettings facebookAuthSettings, IHttpClientFactory httpClientFactory , IConfiguration configuration, ILogger<FacebookAuthService> logger)
        {
            this.facebookAuthSettings = facebookAuthSettings;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            _logger = logger;
        }

        public async Task<FacebookUserInfoResult> GetUserInfoAsync(string accessToken)
        {
            // generate full url
            var formatedUrl = string.Format(UserInfoUrl, accessToken);

            // generate http get request to the specified uri
            var result = await httpClientFactory.CreateClient().GetAsync(formatedUrl);

            result.EnsureSuccessStatusCode();

            // read response message from the get request
            var responseAsString = await result.Content.ReadAsStringAsync();
            // deserialize object
            return JsonConvert.DeserializeObject<FacebookUserInfoResult>(responseAsString);
        }


        public async Task<FacebookTokenValidationResult> ValidateAccessTokenAsync(string accessToken)
        {
            // generate full url
            var formatedUrl = string.Format(TokenValidationUrl, accessToken, facebookAuthSettings.AppId, facebookAuthSettings.AppSecret);

            // get request to specified url
            var result = await httpClientFactory.CreateClient().GetAsync(formatedUrl);
            try
            {
                result.EnsureSuccessStatusCode();
            }catch(Exception e)
            {
                _logger.LogError(e, "Error in Facebook authentication");
            }

            var responseAsString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookTokenValidationResult>(responseAsString);
        }
    }
}
