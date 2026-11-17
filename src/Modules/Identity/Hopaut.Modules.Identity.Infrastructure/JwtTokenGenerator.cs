using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Hopaut.Modules.Identity.Infrastructure;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(
        JwtSettings jwtSettings,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IRefreshTokenRepository refreshTokenRepo,
        TimeProvider timeProvider)
    {
        _jwtSettings = jwtSettings;
        _userManager = userManager;
        _roleManager = roleManager;
        _refreshTokenRepo = refreshTokenRepo;
        _timeProvider = timeProvider;
    }

    public async Task<AuthResult> GenerateForUserAsync(AppUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        var now = _timeProvider.GetUtcNow();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7(now).ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(HopautClaimTypes.UserId, user.Id)
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

        claims.AddRange(userClaims);

        foreach (var roleName in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var roleClaim in roleClaims)
            {
                if (!claims.Any(c => c.Type == roleClaim.Type && c.Value == roleClaim.Value))
                    claims.Add(roleClaim);
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = now.UtcDateTime.Add(_jwtSettings.TokenLifetime),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var refreshToken = new RefreshToken
        {
            JwtId = token.Id,
            UserId = user.Id,
            CreationDate = now.UtcDateTime,
            ExpiryDate = now.UtcDateTime.AddMonths(6)
        };

        _refreshTokenRepo.Add(refreshToken);

        return AuthResult.Ok(user.Id, tokenHandler.WriteToken(token), refreshToken.Token);
    }
}
