using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Posts.Api;

/// <summary>Maps the Posts module's endpoints; its services are registered by <c>AddPostsModule</c> in the module's Infrastructure.</summary>
public static class PostsEndpoints
{
    public static IEndpointRouteBuilder MapPostsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new PostsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
