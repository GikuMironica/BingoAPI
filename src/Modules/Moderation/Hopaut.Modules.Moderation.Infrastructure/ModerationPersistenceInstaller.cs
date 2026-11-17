using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Moderation.Infrastructure;

public static class ModerationPersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddModerationPersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<ModerationModuleDbContext>(options => options.UseModuleNpgsql(connectionString, ModerationModuleDbContext.SchemaName));
}
