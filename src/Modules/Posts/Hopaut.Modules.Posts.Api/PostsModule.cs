using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Application.Commands.DeletePost;
using Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;
using Hopaut.Modules.Posts.Application.Queries.GetPostById;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Posts.Api;

/// <summary>Results map back to today's responses until the Posts endpoints are synchronised in plan Step 10.4 (D-007).</summary>
public sealed class PostsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/posts")
            .WithTags("Posts");

        group.MapGet("/nearby", async (double longitude, double latitude, double? radius, int? limit, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetNearbyPostsQuery(longitude, latitude, radius ?? 10000, limit ?? 50), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        });

        group.MapGet("/{postId:int}", async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetPostByIdQuery(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        });

        group.MapPost("/", async (CreatePostCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/v1/posts/{result.Value.Value}", new { postId = result.Value.Value })
                : result.ToProblem();
        }).RequireAuthorization();

        group.MapDelete("/{postId:int}", async (int postId, HttpContext context, ISender sender, CancellationToken cancellationToken) =>
        {
            var userId = UserId.From(context.User.FindFirst("id")?.Value ?? "");
            var result = await sender.Send(new DeletePostCommand(PostId.From(postId), userId), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();
    }
}
