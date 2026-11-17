using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Moderation.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Moderation.Infrastructure;

public static class ModerationPersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddModerationPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<ModerationModuleDbContext>(options => options.UseModuleNpgsql(connectionString, ModerationModuleDbContext.SchemaName));
        services.AddScoped<IModerationUnitOfWork>(provider => provider.GetRequiredService<ModerationModuleDbContext>());
        return services;
    }
}
