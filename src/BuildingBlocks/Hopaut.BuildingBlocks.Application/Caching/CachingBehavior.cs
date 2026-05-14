using MediatR;
using Microsoft.Extensions.Logging;

namespace Hopaut.BuildingBlocks.Application.Caching;

/// <summary>
/// MediatR pipeline behavior that caches responses for queries implementing <see cref="ICachedQuery"/>.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedQuery)
            return await next();

        var cached = await _cache.GetAsync<TResponse>(cachedQuery.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cachedQuery.CacheKey);
            return cached;
        }

        var response = await next();

        await _cache.SetAsync(cachedQuery.CacheKey, response, cachedQuery.Expiry, cachedQuery.Tags, cancellationToken);
        _logger.LogDebug("Cached response for {CacheKey}", cachedQuery.CacheKey);

        return response;
    }
}
