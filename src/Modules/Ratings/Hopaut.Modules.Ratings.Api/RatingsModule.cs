using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.Modules.Ratings.Application.Commands.CreateRating;
using Hopaut.Modules.Ratings.Application.Commands.DeleteRating;
using Hopaut.Modules.Ratings.Application.Queries.GetRatingById;
using Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Ratings.Api;

/// <summary>Results map back to today's responses until the Ratings endpoints are synchronised in plan Step 10.7 (D-007).</summary>
public sealed class RatingsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/ratings")
            .WithTags("Ratings");

        group.MapGet("/user/{userId}", async (string userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetRatingsByUserQuery(UserId.From(userId)), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        }).RequireAuthorization();

        // Additive (plan Step 8.4): BingoMobile reads a rating with the legacy POST /ratings/{id}; the target is this GET (10.7).
        group.MapGet("/{id:int}", async (int id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetRatingByIdQuery(RatingId.From(id)), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
            .RequireAuthorization()
            .WithName("Ratings_GetRatingById")
            .WithSummary("Gets one rating.");

        // The rater is the caller and the rated user the post's host (D-031): body fields userId and raterId are ignored until 10.7.
        group.MapPost("/", async (CreateRatingCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/v1/ratings/{result.Value.Value}", new { id = result.Value.Value })
                : result.ToProblem();
        }).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteRatingCommand(RatingId.From(id)), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();
    }
}
