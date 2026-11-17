using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.BugReports.Infrastructure;

public sealed class BugReportsDesignTimeDbContextFactory() : ModuleDesignTimeDbContextFactory<BugReportsModuleDbContext>(BugReportsModuleDbContext.SchemaName)
{
    protected override BugReportsModuleDbContext Create(DbContextOptions<BugReportsModuleDbContext> options) => new(options, DesignTimePublisher.Instance);
}
