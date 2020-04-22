using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BingoAPI.Domain;
using BingoAPI.Models;
using BingoAPI.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BingoAPI.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly JwtSettings jwtSettings;
        private readonly UserManager<AppUser> userManager;

        public IdentityService(UserManager<AppUser> userManager,
                               JwtSettings jwtSettings)
        {
            this.userManager = userManager;
            this.jwtSettings = jwtSettings;
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
            var existingUser = await userManager.FindByEmailAsync(email);

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
            var result = await userManager.CreateAsync(newUser, password);

            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Errors = result.Errors.Select(x => x.Description)
                };
            }

            // token generations
            var tokenHandler = new JwtSecurityTokenHandler();

            // Secret is mapped with the one from appsettings.json, binded in startup.class, then jwtSettings added as singleton
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, newUser.Email),
                    new Claim("id", newUser.Id)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}
