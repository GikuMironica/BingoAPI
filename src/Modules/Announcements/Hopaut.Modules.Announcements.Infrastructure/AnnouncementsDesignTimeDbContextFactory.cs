using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Announcements.Infrastructure;

public sealed class AnnouncementsDesignTimeDbContextFactory() : ModuleDesignTimeDbContextFactory<AnnouncementsModuleDbContext>(AnnouncementsModuleDbContext.SchemaName)
{
    protected override AnnouncementsModuleDbContext Create(DbContextOptions<AnnouncementsModuleDbContext> options) => new(options, DesignTimePublisher.Instance);
}
