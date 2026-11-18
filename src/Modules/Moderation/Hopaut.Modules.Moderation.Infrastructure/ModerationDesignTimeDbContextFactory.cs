using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Moderation.Infrastructure;

public sealed class ModerationDesignTimeDbContextFactory() : ModuleDesignTimeDbContextFactory<ModerationModuleDbContext>(ModerationModuleDbContext.SchemaName)
{
    protected override ModerationModuleDbContext Create(DbContextOptions<ModerationModuleDbContext> options) => new(options, DesignTimePublisher.Instance);
}
