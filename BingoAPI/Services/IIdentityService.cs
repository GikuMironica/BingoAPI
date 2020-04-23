using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IIdentityService
    {
        Task<AuthenticationResult> RegisterAsync(string email, string password);

        Task<AuthenticationResult> LoginAsync(string email, string requestPassword);

        Task<AuthenticationResult> RefreshTokenAsync(string token, string requestToken);

        Task<AuthenticationResult> LoginWithFacebookAsync(string accessToken);
    }
}
