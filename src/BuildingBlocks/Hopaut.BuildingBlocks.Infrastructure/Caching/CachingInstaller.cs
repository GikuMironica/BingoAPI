using Hopaut.BuildingBlocks.Application.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.BuildingBlocks.Infrastructure.Caching;

public static class CachingInstaller
{
    public static IServiceCollection AddHopautCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "hopaut:";
            });
        }
        else
        {
            // Fallback to in-memory distributed cache for local development
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
