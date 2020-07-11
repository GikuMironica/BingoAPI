using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Bingo.Contracts.V1.Requests.Identity;
using BingoAPI.Data;
using BingoAPI.Domain;
using BingoAPI.Models;
using BingoAPI.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BingoAPI.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly DataContext _dataContext;
        private readonly IFacebookAuthService facebookAuthService;
        private readonly IUrlHelper _urlHelper;
        private readonly IHttpContextAccessor _httpRequest;
        private readonly IEmailService _emailService;

        public IdentityService(UserManager<AppUser> userManager,
                               JwtSettings jwtSettings,
                               TokenValidationParameters tokenValidationParameters,
                               DataContext dataContext,
                               IFacebookAuthService facebookAuthService,
                               RoleManager<IdentityRole> roleManager,
                               IUrlHelper urlHelper,
                               IHttpContextAccessor httpRequest,
                               IEmailService emailService)
        {
            this._emailService = emailService;
            this._userManager = userManager;
            this._jwtSettings = jwtSettings;
            this._tokenValidationParameters = tokenValidationParameters;
            this._dataContext = dataContext;
            this.facebookAuthService = facebookAuthService;
            this._roleManager = roleManager;
            this._urlHelper = urlHelper;
            this._httpRequest = httpRequest;
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
                UserName = email,
                RegistrationTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
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

            // when registering user, assign him user role, also need to be added in the JWT!!!
            await _userManager.AddToRoleAsync(newUser, "User");

            // force user to confirm email, generate token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

            // generate url
            var confirmationLink = _urlHelper.Action("ConfirmEmail", "Identity",
                    new { userId = newUser.Id, token = token }, _httpRequest.HttpContext.Request.Scheme);

            // send it per email
            var mailresult = 
                await _emailService.SendEmail(newUser.Email, "BingoApp Email Confirmation","Please confirm your account by clicking the link below\n"+confirmationLink);
	    if (mailresult)
                return new AuthenticationResult { Success = true, UserId = newUser.Id };
            else
                return new AuthenticationResult { Success = false, Errors = new List<string> { "Invalid Email Address"} };
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
            if (expiryDateTimeUtc > DateTime.Now)
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

            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("id", user.Id)
                };

            // load all Identity Related claims, roles for this user
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.AddRange(userClaims);

            foreach (var userRole in userRoles)
            {
                // add these roles as claims
                claims.Add(new Claim(ClaimTypes.Role, userRole));

                // if there are any claims associated with this role, get em
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role == null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var roleClaim in roleClaims)
                {
                    // add the claims of this role to the claims list if they are not there yet
                    if (claims.Contains(roleClaim))
                        continue;
                    claims.Add(roleClaim);
                }
            }

            // add this claims in the payload of the token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),                
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

        public async Task<AuthenticationResult> LoginWithFacebookAsync(string accessToken)
        {
            var validatedTokenResult = await facebookAuthService.ValidateAccessTokenAsync(accessToken);

            if (!validatedTokenResult.Data.IsValid)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid Facebook token" }
                };
            }
            var userInfo = await facebookAuthService.GetUserInfoAsync(accessToken);
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            // if email not in the system register user with his FB email
            if(user == null)
            {
                var appUser = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = userInfo.Email,
                    UserName = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    ProfilePicture = userInfo.Picture.Data.Url.ToString(),
                    RegistrationTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                // no password
                var createdResult = await _userManager.CreateAsync(appUser);
                if (!createdResult.Succeeded)
                {
                    return new AuthenticationResult
                    {
                        Errors = new[] { "Something went wrong" }
                    };
                }
                // when registering user, assign him user role, also need to be added in the JWT!!!
                await _userManager.AddToRoleAsync(appUser, "User");
                appUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(appUser);

                return await GenerateAuthenticationResultForUserAsync(appUser);
            }

            // if user already registered with this email, generate jwt for him
            return await GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<AuthenticationResult> RequestNewPasswordAsync(AppUser appUser)
        {
            // generate the reset password token
            var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

            // Build the password reset link
            var passwordResetLink = _urlHelper.Action("ResetPassword", "Identity",
                    new { email = appUser.Email, token = token }, _httpRequest.HttpContext.Request.Scheme);

            // Send link over email
            var result = await _emailService.SendEmail(appUser.Email, "BingoApp", "Click the link below in order to reset your password\n " +
                "A new temporary password will be sent back to this email in a couple of minutes\n" + passwordResetLink);

            if (result)
                return new AuthenticationResult { Success = true };
            else
                return new AuthenticationResult { Success = false, Errors = new List<string> { "Invalid Email" } };
        }

        public async Task<AuthenticationResult> ChangePasswordAsync(AppUser appUser, ChangePasswordRequest request)
        {
            var result = await _userManager.ChangePasswordAsync(appUser, request.OldPass, request.NewPassword);

            // if new password does not meet requirements or current password not correct
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }

            return new AuthenticationResult { Success = true };
        }
    }
}
