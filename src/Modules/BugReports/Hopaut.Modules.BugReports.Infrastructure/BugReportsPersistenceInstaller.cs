using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.BugReports.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.BugReports.Infrastructure;

public static class BugReportsPersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddBugReportsPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<BugReportsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, BugReportsModuleDbContext.SchemaName));
        services.AddScoped<IBugReportsUnitOfWork>(provider => provider.GetRequiredService<BugReportsModuleDbContext>());
        return services;
    }
}
