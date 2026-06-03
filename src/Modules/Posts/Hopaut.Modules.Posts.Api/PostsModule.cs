using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Application.Commands.DeletePost;
using Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;
using Hopaut.Modules.Posts.Application.Queries.GetPostById;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Posts.Api;

public sealed class PostsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/posts")
            .WithTags("Posts");

        group.MapGet("/nearby", async (double longitude, double latitude, double? radius, int? limit, ISender sender) =>
        {
            var result = await sender.Send(new GetNearbyPostsQuery(longitude, latitude, radius ?? 10000, limit ?? 50));
            return Results.Ok(result);
        });

        group.MapGet("/{postId:int}", async (int postId, ISender sender) =>
        {
            var result = await sender.Send(new GetPostByIdQuery(PostId.From(postId)));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapPost("/", async (CreatePostCommand command, ISender sender) =>
        {
            var postId = await sender.Send(command);
            return Results.Created($"/api/v1/posts/{postId.Value}", new { postId = postId.Value });
        }).RequireAuthorization();

        group.MapDelete("/{postId:int}", async (int postId, HttpContext context, ISender sender) =>
        {
            var userId = UserId.From(context.User.FindFirst("id")?.Value ?? "");
            var result = await sender.Send(new DeletePostCommand(PostId.From(postId), userId));
            return result ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();
    }
}
