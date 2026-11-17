using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Hopaut.Modules.Identity.Application;

/// <summary>
/// The validation rules every access token must pass. One definition serves JWT bearer authentication and the refresh
/// flow, which validates an expired token's signature, issuer and audience before issuing a new pair.
/// </summary>
public static class JwtTokenValidation
{
    public static SymmetricSecurityKey SigningKey(JwtSettings settings) => new(Encoding.ASCII.GetBytes(settings.Secret));

    public static TokenValidationParameters Parameters(JwtSettings settings) => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SigningKey(settings),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ValidateIssuer = true,
        ValidIssuer = settings.Issuer,
        ValidateAudience = true,
        ValidAudience = settings.Audience,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = settings.ClockSkew,
    };
}
