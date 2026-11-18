using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Announcements.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Announcements.Infrastructure;

public static class AnnouncementsPersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddAnnouncementsPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AnnouncementsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, AnnouncementsModuleDbContext.SchemaName));
        services.AddScoped<IAnnouncementsUnitOfWork>(provider => provider.GetRequiredService<AnnouncementsModuleDbContext>());
        return services;
    }
}
