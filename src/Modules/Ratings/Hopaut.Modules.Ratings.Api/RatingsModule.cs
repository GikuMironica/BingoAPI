using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Ratings.Application.Commands.CreateRating;
using Hopaut.Modules.Ratings.Application.Commands.DeleteRating;
using Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Ratings.Api;

public sealed class RatingsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/ratings")
            .WithTags("Ratings");

        group.MapGet("/user/{userId}", async (string userId, ISender sender) =>
        {
            var result = await sender.Send(new GetRatingsByUserQuery(UserId.From(userId)));
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/", async (CreateRatingCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/v1/ratings/{id.Value}", new { id = id.Value });
        }).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRatingCommand(RatingId.From(id)));
            return result ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();
    }
}
