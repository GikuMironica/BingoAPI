using BingoAPI.ExternalLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IFacebookAuthService
    {
        Task<FacebookTokenValidationResult> ValidateAccessTokenAsync(String accessToken);

        Task<FacebookUserInfoResult> GetUserInfoAsync(string accessToken);
    }
}
