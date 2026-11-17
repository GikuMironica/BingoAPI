using FluentValidation;
using Hopaut.Modules.BugReports.Application;
using Hopaut.Modules.BugReports.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.BugReports.Api;

/// <summary>Handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class BugReportsModuleInstaller
{
    public static IServiceCollection AddBugReportsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddBugReportsPersistence(connectionString);

        services.AddScoped<IBugReportRepository, BugReportRepository>();

        services.AddValidatorsFromAssemblyContaining<IBugReportsCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapBugReportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new BugReportsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
