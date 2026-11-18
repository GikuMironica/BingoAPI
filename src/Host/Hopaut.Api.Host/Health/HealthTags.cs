namespace Hopaut.Api.Host.Health;

/// <summary>Health-check tags that select the checks of each probe endpoint.</summary>
public static class HealthTags
{
    /// <summary>Dependencies that must be reachable before the instance receives traffic (<c>/health/ready</c>).</summary>
    public const string Ready = "ready";
}
