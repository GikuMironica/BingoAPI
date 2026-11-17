using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hopaut.Api.Host.Health;

/// <summary>Readiness of the distributed cache (Redis, or the in-memory fallback): a probe value must round-trip.</summary>
public sealed class DistributedCacheHealthCheck(IDistributedCache cache) : IHealthCheck
{
    private const string ProbeKey = "health:probe";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.SetStringAsync(
                ProbeKey,
                "ok",
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) },
                cancellationToken);
            var value = await cache.GetStringAsync(ProbeKey, cancellationToken);
            return value == "ok" ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("The cache did not return the probe value.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // A health probe reports the failure instead of throwing; the exception travels with the result.
            return HealthCheckResult.Unhealthy("The distributed cache is unreachable.", exception);
        }
    }
}
