using System.ComponentModel.DataAnnotations;

namespace Hopaut.Modules.Identity.Application;

/// <summary>
/// Access-token settings, bound from <see cref="SectionName"/> and validated when the Host starts. The secret comes from
/// `dotnet user-secrets` or the <c>JwtSettings__Secret</c> environment variable, never from a committed file.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>HMAC-SHA256 signing key: at least 32 characters (256 bits).</summary>
    [Required]
    [MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "Hopaut.Api";

    [Required]
    public string Audience { get; set; } = "Hopaut.Clients";

    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00")]
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(2);

    /// <summary>Tolerated difference between the clocks of the issuer and the validator.</summary>
    [Range(typeof(TimeSpan), "00:00:00", "00:05:00")]
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromSeconds(30);
}
