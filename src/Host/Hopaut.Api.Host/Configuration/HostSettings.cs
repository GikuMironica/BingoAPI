using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Hopaut.Api.Host.Configuration;

/// <summary>Reverse proxies whose X-Forwarded-For / X-Forwarded-Proto headers are trusted; validated at startup.</summary>
public sealed class ProxySettings : IValidatableObject
{
    public const string SectionName = "ProxySettings";

    /// <summary>IP addresses of trusted proxies, e.g. <c>10.0.0.100</c>.</summary>
    public string[] KnownProxies { get; set; } = [];

    /// <summary>Trusted proxy networks in CIDR notation, e.g. <c>10.0.0.0/24</c>.</summary>
    public string[] KnownNetworks { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var proxy in KnownProxies.Where(p => !IPAddress.TryParse(p, out _)))
        {
            yield return new ValidationResult($"'{proxy}' is not an IP address.", [nameof(KnownProxies)]);
        }

        foreach (var network in KnownNetworks.Where(n => !IPNetwork.TryParse(n, out _)))
        {
            yield return new ValidationResult($"'{network}' is not a network in CIDR notation.", [nameof(KnownNetworks)]);
        }
    }
}

/// <summary>Browser origins allowed by CORS. CORS is enabled only when at least one origin is configured.</summary>
public sealed class CorsSettings : IValidatableObject
{
    public const string SectionName = "Cors";

    /// <summary>Absolute http(s) origins without a path, e.g. <c>https://admin.hopaut.app</c>.</summary>
    public string[] AllowedOrigins { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var origin in AllowedOrigins.Where(o => !IsOrigin(o)))
        {
            yield return new ValidationResult($"'{origin}' is not an http(s) origin.", [nameof(AllowedOrigins)]);
        }
    }

    private static bool IsOrigin(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https"
        && uri.PathAndQuery == "/"
        && !value.EndsWith('/');
}
