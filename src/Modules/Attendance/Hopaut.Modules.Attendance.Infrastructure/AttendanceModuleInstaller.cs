using FluentValidation;
using Hopaut.Modules.Attendance.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Attendance.Infrastructure;

/// <summary>Registers the Attendance module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class AttendanceModuleInstaller
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAttendancePersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();

        services.AddValidatorsFromAssemblyContaining<IAttendanceCommand>(includeInternalTypes: true);

        return services;
    }
}
