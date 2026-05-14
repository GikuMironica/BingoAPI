using System.Text.Json;
using Hopaut.BuildingBlocks.Application.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace Hopaut.BuildingBlocks.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, string[]? tags = null, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
        };
        await _cache.SetAsync(key, bytes, options, ct);

        // Track tags → keys for invalidation
        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                var tagKey = $"tag:{tag}";
                var existing = await _cache.GetStringAsync(tagKey, ct) ?? "";
                var keys = existing.Length > 0 ? existing + "," + key : key;
                await _cache.SetStringAsync(tagKey, keys, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
                }, ct);
            }
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
    }

    public async Task InvalidateByTagAsync(string tag, CancellationToken ct = default)
    {
        var tagKey = $"tag:{tag}";
        var keysStr = await _cache.GetStringAsync(tagKey, ct);
        if (string.IsNullOrEmpty(keysStr)) return;

        var keys = keysStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var key in keys)
        {
            await _cache.RemoveAsync(key, ct);
        }
        await _cache.RemoveAsync(tagKey, ct);
    }
}
