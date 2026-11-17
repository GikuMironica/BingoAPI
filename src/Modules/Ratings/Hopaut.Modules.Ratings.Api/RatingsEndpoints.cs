using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Ratings.Api;

/// <summary>Maps the Ratings module's endpoints; its services are registered by <c>AddRatingsModule</c> in the module's Infrastructure.</summary>
public static class RatingsEndpoints
{
    public static IEndpointRouteBuilder MapRatingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new RatingsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
