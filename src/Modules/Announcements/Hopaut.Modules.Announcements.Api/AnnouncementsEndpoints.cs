using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Announcements.Api;

/// <summary>Maps the Announcements module's endpoints; its services are registered by <c>AddAnnouncementsModule</c> in the module's Infrastructure.</summary>
public static class AnnouncementsEndpoints
{
    public static IEndpointRouteBuilder MapAnnouncementsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new AnnouncementsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
