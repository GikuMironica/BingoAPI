using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hopaut.BuildingBlocks.Application.Security;
using Microsoft.IdentityModel.Tokens;

namespace Hopaut.IntegrationTests.Infrastructure;

/// <summary>Issues access tokens shaped like the Identity module's, so tests authenticate without the login flow.</summary>
public static class TestJwt
{
    /// <summary>Test-only signing key, set by <see cref="HopautApiFactory"/>; never used outside the test host.</summary>
    public const string Secret = "integration-tests-only-signing-key-0123456789";

    public const string Issuer = "Hopaut.Api";
    public const string Audience = "Hopaut.Clients";

    public static string Create(
        string userId,
        IEnumerable<string>? roles = null,
        string issuer = Issuer,
        string audience = Audience,
        string secret = Secret,
        DateTimeOffset? expires = null)
    {
        var now = DateTimeOffset.UtcNow;
        var expiry = expires ?? now.AddMinutes(10);
        var issuedAt = expiry < now ? expiry.AddMinutes(-10) : now;
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(HopautClaimTypes.UserId, userId),
            .. (roles ?? []).Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiry.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature),
        });
        return handler.WriteToken(token);
    }
}
