using FluentValidation;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Announcements.Api;

/// <summary>Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class AnnouncementsModuleInstaller
{
    public static IServiceCollection AddAnnouncementsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddAnnouncementsPersistence(connectionString);

        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

        services.AddValidatorsFromAssemblyContaining<IAnnouncementsCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapAnnouncementsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new AnnouncementsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
