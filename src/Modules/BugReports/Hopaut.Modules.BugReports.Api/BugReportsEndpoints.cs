using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.BugReports.Api;

/// <summary>Maps the BugReports module's endpoints; its services are registered by <c>AddBugReportsModule</c> in the module's Infrastructure.</summary>
public static class BugReportsEndpoints
{
    public static IEndpointRouteBuilder MapBugReportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new BugReportsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
