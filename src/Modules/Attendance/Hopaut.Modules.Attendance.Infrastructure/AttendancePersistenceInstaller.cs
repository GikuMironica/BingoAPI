using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Attendance.Infrastructure;

public static class AttendancePersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddAttendancePersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<AttendanceModuleDbContext>(options => options.UseModuleNpgsql(connectionString, AttendanceModuleDbContext.SchemaName));
}
