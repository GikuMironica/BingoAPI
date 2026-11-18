using Mediator;

namespace Hopaut.BuildingBlocks.Application.Caching;

/// <summary>A query whose successful result <see cref="CachingBehavior{TMessage,TResponse}"/> caches under <see cref="CacheKey"/>.</summary>
public interface ICachedQuery : IBaseQuery
{
    string CacheKey { get; }

    TimeSpan? Expiry => null;

    string[]? Tags => null;
}
