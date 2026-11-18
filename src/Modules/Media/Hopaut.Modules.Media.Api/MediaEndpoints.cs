using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Media.Api;

/// <summary>Maps the Media module's endpoints; its services are registered by <c>AddMediaModule</c> in the module's Infrastructure.</summary>
public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new MediaModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
