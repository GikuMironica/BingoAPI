using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Identity.Infrastructure;

public static class IdentityPersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddIdentityPersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<IdentityModuleDbContext>(options => options.UseModuleNpgsql(connectionString, IdentityModuleDbContext.SchemaName));
}
