using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Announcements.Api;

/// <summary>Results map back to today's responses until the Announcements endpoints are synchronised in plan Step 10.6 (D-007).</summary>
public sealed class AnnouncementsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/announcements")
            .WithTags("Announcements");

        group.MapGet("/post/{postId:int}", async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAnnouncementsByPostQuery(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        }).RequireAuthorization();

        group.MapPost("/", async (CreateAnnouncementCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/v1/announcements/{result.Value.Value}", new { id = result.Value.Value })
                : result.ToProblem();
        }).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteAnnouncementCommand(AnnouncementId.From(id)), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();
    }
}
