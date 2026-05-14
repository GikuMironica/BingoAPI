using Hopaut.Modules.BugReports.Application;
using Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;
using Hopaut.Modules.BugReports.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.BugReports.Api;

public static class BugReportsModuleInstaller
{
    public static IServiceCollection AddBugReportsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<BugReportsModuleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", BugReportsModuleDbContext.SchemaName)));

        services.AddScoped<IBugReportRepository, BugReportRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateBugReportCommand>());

        return services;
    }

    public static IEndpointRouteBuilder MapBugReportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new BugReportsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
