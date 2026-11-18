namespace Hopaut.BuildingBlocks.Application.Caching;

/// <summary>
/// Abstraction over distributed cache with tag-based invalidation.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, string[]? tags = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task InvalidateByTagAsync(string tag, CancellationToken ct = default);
}
