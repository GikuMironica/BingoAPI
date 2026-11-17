using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Announcements.Infrastructure;

public static class AnnouncementsPersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddAnnouncementsPersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<AnnouncementsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, AnnouncementsModuleDbContext.SchemaName));
}
