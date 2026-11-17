using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Attendance.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Attendance.Infrastructure;

public static class AttendancePersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddAttendancePersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AttendanceModuleDbContext>(options => options.UseModuleNpgsql(connectionString, AttendanceModuleDbContext.SchemaName));
        services.AddScoped<IAttendanceUnitOfWork>(provider => provider.GetRequiredService<AttendanceModuleDbContext>());
        return services;
    }
}
