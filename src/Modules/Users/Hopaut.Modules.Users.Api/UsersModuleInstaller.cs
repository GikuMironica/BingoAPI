using FluentValidation;
using Hopaut.Modules.Users.Application;
using Hopaut.Modules.Users.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Users.Api;

/// <summary>Handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class UsersModuleInstaller
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddUsersPersistence(connectionString);

        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        services.AddValidatorsFromAssemblyContaining<IUsersCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new UsersModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
