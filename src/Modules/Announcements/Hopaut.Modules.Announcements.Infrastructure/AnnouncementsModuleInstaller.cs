using FluentValidation;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Posts.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Announcements.Infrastructure;

/// <summary>Registers the Announcements module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator; the post host comes from the Posts module's IPostHostQueries.</summary>
public static class AnnouncementsModuleInstaller
{
    public static IServiceCollection AddAnnouncementsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAnnouncementsPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

        services.AddResourceOwnerPolicy<PostHost>(AnnouncementPolicies.MustOwnAnnouncement, host => host.HostId, administratorsAllowed: true);

        services.AddValidatorsFromAssemblyContaining<IAnnouncementsCommand>(includeInternalTypes: true);

        return services;
    }
}
