namespace Hopaut.Modules.Identity.Application;

public sealed class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = "Hopaut.Api";
    public string Audience { get; set; } = "Hopaut.Clients";
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(2);
}
