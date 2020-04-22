using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BingoAPI.Data;
using BingoAPI.Domain;
using BingoAPI.Models;
using BingoAPI.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BingoAPI.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly DataContext _dataContext;

        public IdentityService(UserManager<AppUser> userManager,
                               JwtSettings jwtSettings,
                               TokenValidationParameters tokenValidationParameters,
                               DataContext dataContext)
        {
            this._userManager = userManager;
            this._jwtSettings = jwtSettings;
            this._tokenValidationParameters = tokenValidationParameters;
            this._dataContext = dataContext;
        }


        /// <summary>
        /// This method registers a user in the system,
        /// with the given email and password which is then hashed.
        /// </summary>
        /// <param name="email">User registration email</param>
        /// <param name="password">User password</param>
        /// <returns>This method returns the generaterd Jwt token
        /// if opeartion was successful</returns>
        public async Task<AuthenticationResult> RegisterAsync(string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if(existingUser != null)
            {
                return new AuthenticationResult { Errors = new[] { "User with this email address exists" } };
            }

            // generate user
            var newUser = new AppUser
            {
                Email = email,
                UserName = email
            };

            // register user in system
            var result = await _userManager.CreateAsync(newUser, password);

            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Errors = result.Errors.Select(x => x.Description)
                };
            }
            return await GenerateAuthenticationResultForUserAsync(newUser);            
        }


        /// <summary>
        /// This method Logs in existing user in the system
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="requestPassword">User password</param>
        /// <returns>Return AuthenticationResult</returns>
        public async Task<AuthenticationResult> LoginAsync(string email, string requestPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if(user == null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "User does not exist" }
                };
            }

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, requestPassword);
            if (!userHasValidPassword)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Username / Password combination is wrong" }
                };
            }
            return await GenerateAuthenticationResultForUserAsync(user);
        }


        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string requestToken)
        {
            var validatedToken = GetPrincipalFromToken(token);

            if (validatedToken == null)
            {
                return new AuthenticationResult { Errors = new[] { "Invalid Token" } };
            }

            // check if token expired
            var expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDateUnix).ToLocalTime();

            // if not expired, throw error 
            if (expiryDateTimeUtc < DateTime.UtcNow)
            {
                return new AuthenticationResult { Errors = new[] { "This token hasn't expired yet" } };
            }

            // get JTI ID from claim ( id of  the token )
            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            // get the refresh token from db
            var storedRefreshToken = await _dataContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == requestToken);

            // if doesnt exist, client must login
            if (storedRefreshToken == null)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token does not exist" } };
            }

            // if exists, check expiry date of refresh token, client must login
            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token has expired" } };
            }

            // if it has been invalidated, client must login
            if (storedRefreshToken.Invalidated)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token has been invalidated" } };
            }

            // check if it was used
            if (storedRefreshToken.Used)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token has been used" } };
            }

            if (storedRefreshToken.JwtId != jti)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token doesn not match this JWT" } };
            }

            storedRefreshToken.Used = true;
            _dataContext.RefreshTokens.Update(storedRefreshToken);
            await _dataContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(x => x.Type == "id").Value);

            // return new JWT+Refresh token fro user
            return await GenerateAuthenticationResultForUserAsync(user);

        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenValidationParams = _tokenValidationParameters.Clone();
                tokenValidationParams.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out var validatedToken);

                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }
        
        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Generates jwt , refresh token for <paramref name="user"/> 
        /// </summary>
        /// <param name="user">An identity user</param>
        /// <returns></returns>
        private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(AppUser user)
        {
            // token generations
            var tokenHandler = new JwtSecurityTokenHandler();

            // Secret is mapped with the one from appsettings.json, binded in startup.class, then jwtSettings added as singleton
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("id", user.Id)
                }),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            var result = await _dataContext.RefreshTokens.AddAsync(refreshToken);
            await _dataContext.SaveChangesAsync();
                        

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token
            };
        }

        
    }
}
