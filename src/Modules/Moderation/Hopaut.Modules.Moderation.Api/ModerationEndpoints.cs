using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Moderation.Api;

/// <summary>Maps the Moderation module's endpoints; its services are registered by <c>AddModerationModule</c> in the module's Infrastructure.</summary>
public static class ModerationEndpoints
{
    public static IEndpointRouteBuilder MapModerationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new ModerationModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
