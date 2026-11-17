using FluentValidation;
using Hopaut.Modules.Announcements.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Announcements.Infrastructure;

/// <summary>Registers the Announcements module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class AnnouncementsModuleInstaller
{
    public static IServiceCollection AddAnnouncementsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAnnouncementsPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

        services.AddValidatorsFromAssemblyContaining<IAnnouncementsCommand>(includeInternalTypes: true);

        return services;
    }
}
