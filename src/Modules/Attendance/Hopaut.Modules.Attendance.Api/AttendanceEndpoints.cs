using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Attendance.Api;

/// <summary>Maps the Attendance module's endpoints; its services are registered by <c>AddAttendanceModule</c> in the module's Infrastructure.</summary>
public static class AttendanceEndpoints
{
    public static IEndpointRouteBuilder MapAttendanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new AttendanceModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
