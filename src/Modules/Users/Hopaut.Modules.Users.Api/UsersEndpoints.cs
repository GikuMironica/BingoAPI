using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Users.Api;

/// <summary>Maps the Users module's endpoints; its services are registered by <c>AddUsersModule</c> in the module's Infrastructure.</summary>
public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new UsersModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
