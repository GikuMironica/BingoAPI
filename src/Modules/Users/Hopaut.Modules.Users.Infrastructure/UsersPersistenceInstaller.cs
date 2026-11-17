using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Users.Infrastructure;

public static class UsersPersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddUsersPersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<UsersModuleDbContext>(options => options.UseModuleNpgsql(connectionString, UsersModuleDbContext.SchemaName));
}
