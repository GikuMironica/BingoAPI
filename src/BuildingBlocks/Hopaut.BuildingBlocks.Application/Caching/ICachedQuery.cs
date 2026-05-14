namespace Hopaut.BuildingBlocks.Application.Caching;

/// <summary>
/// Marker interface for MediatR queries that should be cached.
/// </summary>
public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan? Expiry => null;
    string[]? Tags => null;
}
