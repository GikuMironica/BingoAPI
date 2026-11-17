using FluentValidation;
using Hopaut.Modules.Moderation.Application;
using Hopaut.Modules.Moderation.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Moderation.Api;

/// <summary>Handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class ModerationModuleInstaller
{
    public static IServiceCollection AddModerationModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddModerationPersistence(connectionString);

        services.AddScoped<IModerationRepository, ModerationRepository>();

        services.AddValidatorsFromAssemblyContaining<IModerationCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapModerationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new ModerationModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
