using FluentValidation;
using Hopaut.Modules.Moderation.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Moderation.Infrastructure;

/// <summary>Registers the Moderation module's services. Handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class ModerationModuleInstaller
{
    public static IServiceCollection AddModerationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModerationPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IModerationRepository, ModerationRepository>();

        services.AddValidatorsFromAssemblyContaining<IModerationCommand>(includeInternalTypes: true);

        return services;
    }
}
