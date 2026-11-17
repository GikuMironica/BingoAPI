using Hangfire.Dashboard;
using Hopaut.BuildingBlocks.Application.Security;

namespace Hopaut.BuildingBlocks.Infrastructure.Hangfire;

/// <summary>
/// Opens the Hangfire dashboard to administrators only (finding F-015). Hangfire answers 401 to anonymous callers and
/// 403 to authenticated users without an administrator role.
/// </summary>
public sealed class AdminDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;
        return user.Identity?.IsAuthenticated == true
            && (user.IsInRole(HopautRoles.Admin) || user.IsInRole(HopautRoles.SuperAdmin));
    }
}
