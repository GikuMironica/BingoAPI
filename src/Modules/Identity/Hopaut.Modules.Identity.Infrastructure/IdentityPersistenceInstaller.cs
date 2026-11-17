using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Identity.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Identity.Infrastructure;

public static class IdentityPersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddIdentityPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<IdentityModuleDbContext>(options => options.UseModuleNpgsql(connectionString, IdentityModuleDbContext.SchemaName));
        services.AddScoped<IIdentityUnitOfWork>(provider => provider.GetRequiredService<IdentityModuleDbContext>());
        return services;
    }
}
