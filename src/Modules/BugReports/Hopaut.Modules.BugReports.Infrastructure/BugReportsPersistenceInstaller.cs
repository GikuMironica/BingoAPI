using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.BugReports.Infrastructure;

public static class BugReportsPersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddBugReportsPersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<BugReportsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, BugReportsModuleDbContext.SchemaName));
}
