using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

public sealed class RatingsDesignTimeDbContextFactory() : ModuleDesignTimeDbContextFactory<RatingsModuleDbContext>(RatingsModuleDbContext.SchemaName)
{
    protected override RatingsModuleDbContext Create(DbContextOptions<RatingsModuleDbContext> options) => new(options, DesignTimePublisher.Instance);
}
