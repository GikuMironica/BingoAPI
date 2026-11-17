using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Identity.Api;

/// <summary>Maps the Identity module's endpoints; its services are registered by <c>AddIdentityModule</c> in the module's Infrastructure.</summary>
public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new IdentityModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
