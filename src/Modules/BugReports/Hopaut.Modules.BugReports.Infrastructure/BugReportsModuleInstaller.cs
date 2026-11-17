using FluentValidation;
using Hopaut.Modules.BugReports.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.BugReports.Infrastructure;

/// <summary>Registers the BugReports module's services. Handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class BugReportsModuleInstaller
{
    public static IServiceCollection AddBugReportsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBugReportsPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IBugReportRepository, BugReportRepository>();

        services.AddValidatorsFromAssemblyContaining<IBugReportsCommand>(includeInternalTypes: true);

        return services;
    }
}
