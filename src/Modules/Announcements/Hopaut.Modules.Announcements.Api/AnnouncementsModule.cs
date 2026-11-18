using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.Modules.Announcements.Api.Contracts.V1;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;
using Hopaut.Modules.Announcements.Application.Commands.UpdateAnnouncement;
using Hopaut.Modules.Announcements.Application.Queries;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementById;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementThreads;
using Hopaut.Modules.Announcements.Application.Queries.GetPostAnnouncements;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Announcements.Api;

/// <summary>
/// Results map back to today's responses until the Announcements endpoints are synchronised in plan Step 10.6 (D-007): the
/// failures an endpoint already had keep their response (the empty 404 of a delete), and the failure paths added in Step
/// 8.5 answer problem details (D-021, D-033).
/// </summary>
public sealed class AnnouncementsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(AnnouncementsRoutes.Announcements)
            .WithTags("Announcements")
            .RequireAuthorization();

        group.MapGet(AnnouncementsRoutes.ByPost, async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAnnouncementsByPostQuery(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value.Select(ToResponse).ToList()) : result.ToProblem();
        });

        group.MapPost(AnnouncementsRoutes.Create, async (CreateAnnouncementCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created(AnnouncementsRoutes.Announcement(result.Value.Value), new { id = result.Value.Value })
                : result.ToProblem();
        });

        // Additive (plan Step 8.5): BingoMobile reads one announcement with the legacy GET /announcements/{id}.
        group.MapGet(AnnouncementsRoutes.ById, async Task<Results<Ok<AnnouncementResponse>, ProblemHttpResult>> (
            int announcementId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAnnouncementByIdQuery(AnnouncementId.From(announcementId)), cancellationToken);
            return result.IsSuccess ? TypedResults.Ok(ToResponse(result.Value)) : result.ToProblem();
        })
            .WithName("Announcements_GetAnnouncement")
            .WithSummary("Gets one announcement. The post's host, its accepted attendees and administrators only.")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Additive (plan Step 8.5): BingoMobile edits with the legacy PUT /announcements/{id}; the answer is the edited announcement.
        group.MapPut(AnnouncementsRoutes.ById, async Task<Results<Ok<AnnouncementResponse>, ProblemHttpResult>> (
            int announcementId, UpdateAnnouncementRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = AnnouncementId.From(announcementId);
            var updated = await sender.Send(new UpdateAnnouncementCommand(id, request.Message), cancellationToken);
            if (updated.IsFailure)
                return updated.ToProblem();

            var result = await sender.Send(new GetAnnouncementByIdQuery(id), cancellationToken);
            return result.IsSuccess ? TypedResults.Ok(ToResponse(result.Value)) : result.ToProblem();
        })
            .WithName("Announcements_UpdateAnnouncement")
            .WithSummary("Replaces the message of an announcement. The post's host or an administrator only.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete(AnnouncementsRoutes.ById, async (int announcementId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteAnnouncementCommand(AnnouncementId.From(announcementId)), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : NotFoundOrProblem(result);
        });

        // Additive (plan Step 8.5): the paged successor of GET /api/v1/announcements/post/{postId}.
        endpoints.MapGet(AnnouncementsRoutes.PostAnnouncements, async Task<Results<Ok<AnnouncementPageResponse>, ProblemHttpResult>> (
            int postId, string? cursor, int? limit, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetPostAnnouncementsQuery(PostId.From(postId), cursor, limit), cancellationToken);
            return result.IsSuccess
                ? TypedResults.Ok(new AnnouncementPageResponse(result.Value.Items.Select(ToResponse).ToList(), result.Value.NextCursor))
                : result.ToProblem();
        })
            .RequireAuthorization()
            .WithTags("Announcements")
            .WithName("Announcements_GetPostAnnouncements")
            .WithSummary("Lists a post's announcements, newest first, cursor-paged. The post's host, its accepted attendees and administrators only.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Additive (plan Step 8.5): replaces the legacy GET /announcements/inbox and /announcements/outbox.
        endpoints.MapGet(AnnouncementsRoutes.MyThreads, async Task<Results<Ok<AnnouncementThreadPageResponse>, ProblemHttpResult>> (
            string? box, string? cursor, int? limit, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAnnouncementThreadsQuery(box, cursor, limit), cancellationToken);
            return result.IsSuccess
                ? TypedResults.Ok(new AnnouncementThreadPageResponse(result.Value.Items.Select(ToResponse).ToList(), result.Value.NextCursor))
                : result.ToProblem();
        })
            .RequireAuthorization()
            .WithTags("Announcements")
            .WithName("Announcements_GetMyThreads")
            .WithSummary("Lists the caller's announcement threads: box=inbox (default; posts the caller attends) or outbox (posts the caller hosts), most recently announced first, cursor-paged.")
            .ProducesValidationProblem();
    }

    private static AnnouncementResponse ToResponse(AnnouncementDto announcement) => new(
        announcement.Id.Value, announcement.PostId.Value, announcement.Message, announcement.CreatedAt, announcement.EditedAt);

    private static AnnouncementThreadResponse ToResponse(AnnouncementThread thread) =>
        new(thread.PostId.Value, ToResponse(thread.LastAnnouncement));

    /// <summary>Today's empty 404 for a missing announcement or post and for a caller who is neither the host nor an administrator.</summary>
    private static IResult NotFoundOrProblem(Result result) =>
        result.Error.Type is ErrorType.NotFound or ErrorType.Forbidden ? Results.NotFound() : result.ToProblem();
}
