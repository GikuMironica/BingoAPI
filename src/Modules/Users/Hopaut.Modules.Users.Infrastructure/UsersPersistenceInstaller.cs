using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Users.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Users.Infrastructure;

public static class UsersPersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddUsersPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<UsersModuleDbContext>(options => options.UseModuleNpgsql(connectionString, UsersModuleDbContext.SchemaName));
        services.AddScoped<IUsersUnitOfWork>(provider => provider.GetRequiredService<UsersModuleDbContext>());
        return services;
    }
}
