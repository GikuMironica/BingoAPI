using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

public sealed class AttendanceDesignTimeDbContextFactory() : ModuleDesignTimeDbContextFactory<AttendanceModuleDbContext>(AttendanceModuleDbContext.SchemaName)
{
    protected override AttendanceModuleDbContext Create(DbContextOptions<AttendanceModuleDbContext> options) => new(options, DesignTimePublisher.Instance);
}
