using FluentValidation;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Attendance.Api;

/// <summary>Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class AttendanceModuleInstaller
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddAttendancePersistence(connectionString);

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();

        services.AddValidatorsFromAssemblyContaining<IAttendanceCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapAttendanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new AttendanceModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
