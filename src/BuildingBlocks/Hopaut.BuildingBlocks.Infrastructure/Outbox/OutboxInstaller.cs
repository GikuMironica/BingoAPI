using Hopaut.BuildingBlocks.Application;
using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

public static class OutboxInstaller
{
    /// <summary>
    /// Registers the transitional shared outbox so integration events are persisted. The relay (<see cref="OutboxPublisher"/>)
    /// is deliberately not registered: it marks messages processed without publishing them (finding F-005, plan Steps 9.1/9.2).
    /// </summary>
    public static IServiceCollection AddHopautOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OutboxDbContext>(options =>
            options.UseModuleNpgsql(configuration.GetConnectionString("DefaultConnection"), OutboxDbContext.SchemaName));
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
        return services;
    }
}
