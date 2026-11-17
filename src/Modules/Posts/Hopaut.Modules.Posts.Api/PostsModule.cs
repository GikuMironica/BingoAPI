using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Posts.Api.Contracts.V1;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Application.Commands.DeletePost;
using Hopaut.Modules.Posts.Application.Commands.DisablePost;
using Hopaut.Modules.Posts.Application.Commands.UpdatePost;
using Hopaut.Modules.Posts.Application.Queries.GetMyAttendances;
using Hopaut.Modules.Posts.Application.Queries.GetMyPosts;
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

        // Today's response is the list (D-007); the page cursor is exposed with the feed's sync in plan Step 10.4.
        group.MapGet("/nearby", async (double longitude, double latitude, double? radius, int? limit, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetNearbyPostsQuery(longitude, latitude, radius ?? 10000, limit ?? 50), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value.Items) : result.ToProblem();
        });

        group.MapGet("/{postId:int}", async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetPostByIdQuery(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        });

        // The host is the caller (F-013): a userId sent in the body is ignored until the body shape changes in plan Step 10.4.
        group.MapPost("/", async (CreatePostCommand command, ICurrentUser currentUser, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command with { UserId = currentUser.Id }, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/v1/posts/{result.Value.Value}", new { postId = result.Value.Value })
                : result.ToProblem();
        }).RequireAuthorization();

        group.MapDelete("/{postId:int}", async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeletePostCommand(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();

        // Additive (plan Step 8.2): the legacy update had no module endpoint yet; failures are problem details.
        group.MapPatch("/{postId:int}", async (int postId, UpdatePostRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(ToCommand(PostId.From(postId), request), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblem();
        })
            .RequireAuthorization()
            .WithName("Posts_UpdatePost")
            .WithSummary("Partially updates a post. Host or administrator only.");

        // Additive (plan Step 8.2): the legacy admin "disable" (PUT /posts/disable with { id }) as a status transition.
        group.MapPatch("/{postId:int}/status", async (int postId, UpdatePostStatusRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            if (!string.Equals(request.Status, DisabledStatus, StringComparison.OrdinalIgnoreCase))
                return new ValidationError(new Dictionary<string, string[]> { ["status"] = [$"The only supported status is \"{DisabledStatus}\"."] }).ToProblem();

            var result = await sender.Send(new DisablePostCommand(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblem();
        })
            .RequireAuthorization(HopautPolicies.Admin)
            .WithName("Posts_UpdatePostStatus")
            .WithSummary("Disables a post. Administrators only.");

        // Additive (plan Step 8.2): replaces the legacy GET /posts/myactive and /posts/myinactive, scoped to the caller (F-067).
        endpoints.MapGet("/api/v1/users/me/posts", async (string? status, string? cursor, int? limit, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetMyPostsQuery(status, cursor, limit), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("Posts_GetMyPosts")
            .WithSummary("Lists the caller's posts: status=active (default) or inactive, newest start first, cursor-paged.");

        // Additive (plan Step 8.3): replaces the legacy GET /attendedactive and /attendedinactive (accepted participations of the caller).
        endpoints.MapGet("/api/v1/users/me/attendances", async (string? status, string? cursor, int? limit, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetMyAttendancesQuery(status, cursor, limit), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("Posts_GetMyAttendances")
            .WithSummary("Lists the posts the caller attends: status=active (default) or inactive, newest start first, cursor-paged.");
    }

    private const string DisabledStatus = "disabled";

    private static UpdatePostCommand ToCommand(PostId postId, UpdatePostRequest request) => new(
        postId,
        request.EventTime,
        request.EndTime,
        request.Location is { } location
            ? new CreatePostLocationInput(location.EntityName, location.City, location.Region, location.Address, location.Country, location.Longitude, location.Latitude)
            : null,
        request.Event is { } @event
            ? new UpdatePostEventInput(@event.Title, @event.Description, @event.Requirements, @event.Slots, @event.EntrancePrice, @event.Currency)
            : null,
        request.TagNames);
}
