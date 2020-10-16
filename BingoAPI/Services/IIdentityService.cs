using Bingo.Contracts.V1.Requests.Identity;
using BingoAPI.Domain;
using BingoAPI.Models;
using System;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IIdentityService
    {
        Task<AuthenticationResult> RegisterAsync(string email, string password, String? lang = null);

        Task<AuthenticationResult> LoginAsync(string email, string requestPassword);

        Task<AuthenticationResult> RequestNewPasswordAsync(AppUser appUser);

        Task<AuthenticationResult> ChangePasswordAsync(AppUser appUser, ChangePasswordRequest request);

        Task<AuthenticationResult> RefreshTokenAsync(string token, string requestToken);

        Task<AuthenticationResult> LoginWithFacebookAsync(string accessToken);
    }
}
