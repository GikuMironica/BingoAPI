using FluentValidation;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Announcements.Application.Queries;
using Hopaut.Modules.Posts.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Announcements.Infrastructure;

/// <summary>
/// Registers the Announcements module's services. Handlers, domain-event handlers and the unit-of-work behavior are
/// registered by the Host's AddMediator; post hosts come from the Posts module's IPostHostQueries and accepted attendances
/// from the Attendance module's IAttendanceQueries.
/// </summary>
public static class AnnouncementsModuleInstaller
{
    public static IServiceCollection AddAnnouncementsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAnnouncementsPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IAnnouncementReadModel, AnnouncementReadModel>();

        services.AddResourceOwnerPolicy<PostHost>(AnnouncementPolicies.MustOwnAnnouncement, host => host.HostId, administratorsAllowed: true);

        services.AddValidatorsFromAssemblyContaining<IAnnouncementsCommand>(includeInternalTypes: true);

        return services;
    }
}
