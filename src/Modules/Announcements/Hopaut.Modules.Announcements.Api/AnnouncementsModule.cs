using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Announcements.Api;

public sealed class AnnouncementsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/announcements")
            .WithTags("Announcements");

        group.MapGet("/post/{postId:int}", async (int postId, ISender sender) =>
        {
            var result = await sender.Send(new GetAnnouncementsByPostQuery(PostId.From(postId)));
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/", async (CreateAnnouncementCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/v1/announcements/{id.Value}", new { id = id.Value });
        }).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteAnnouncementCommand(AnnouncementId.From(id)));
            return result ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();
    }
}
