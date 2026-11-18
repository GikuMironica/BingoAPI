using FluentValidation;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Attendance.Application.Queries;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Attendance.Infrastructure;

/// <summary>Registers the Attendance module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator; a post's host and attendance terms come from the Posts module's IPostAttendanceTermsQueries.</summary>
public static class AttendanceModuleInstaller
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAttendancePersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAttendanceReadModel, AttendanceReadModel>();
        services.AddScoped<IAttendanceQueries, AttendanceQueries>();

        services.AddResourceOwnerPolicy<PostHost>(AttendancePolicies.MustHostPost, host => host.HostId, administratorsAllowed: false);

        services.AddValidatorsFromAssemblyContaining<IAttendanceCommand>(includeInternalTypes: true);

        return services;
    }
}
