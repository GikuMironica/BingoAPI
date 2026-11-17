using Hopaut.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Hopaut.BuildingBlocks.Application.Caching;

/// <summary>
/// Serves <see cref="ICachedQuery"/> queries from the cache and caches their successful results; failures are never cached.
/// Selected by the generic constraint, so other messages never reach it.
/// </summary>
public sealed class CachingBehavior<TMessage, TResponse>(ICacheService cache, ILogger<CachingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : ICachedQuery
    where TResponse : IResultBase
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<TResponse>(message.CacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", message.CacheKey);
            return cached;
        }

        var response = await next(message, cancellationToken);
        if (response.IsSuccess)
        {
            await cache.SetAsync(message.CacheKey, response, message.Expiry, message.Tags, cancellationToken);
            logger.LogDebug("Cached response for {CacheKey}", message.CacheKey);
        }

        return response;
    }
}
