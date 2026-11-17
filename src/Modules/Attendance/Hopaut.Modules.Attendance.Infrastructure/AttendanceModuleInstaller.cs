using FluentValidation;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Posts.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Attendance.Infrastructure;

/// <summary>Registers the Attendance module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator; the post host comes from the Posts module's IPostHostQueries.</summary>
public static class AttendanceModuleInstaller
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAttendancePersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();

        services.AddResourceOwnerPolicy<PostHost>(AttendancePolicies.MustHostPost, host => host.HostId, administratorsAllowed: false);

        services.AddValidatorsFromAssemblyContaining<IAttendanceCommand>(includeInternalTypes: true);

        return services;
    }
}
