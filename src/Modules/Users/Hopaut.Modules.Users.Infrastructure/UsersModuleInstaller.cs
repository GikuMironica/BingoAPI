using FluentValidation;
using Hopaut.Modules.Users.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Users.Infrastructure;

/// <summary>Registers the Users module's services. Handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class UsersModuleInstaller
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUsersPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        services.AddValidatorsFromAssemblyContaining<IUsersCommand>(includeInternalTypes: true);

        return services;
    }
}
