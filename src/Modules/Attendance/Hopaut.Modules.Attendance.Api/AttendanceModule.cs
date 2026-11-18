using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Attendance.Api.Contracts.V1;
using Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;
using Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Attendance.Application.Queries;
using Hopaut.Modules.Attendance.Application.Queries.GetAttendees;
using Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Attendance.Api;

/// <summary>
/// Results map back to today's responses until the Attendance endpoints are synchronised in plan Step 10.5 (D-007): the
/// failures an endpoint already had keep their status (409 "Already requested" for a request, 404 for a decision or a
/// cancellation), and the failure paths added in Step 8.3 answer problem details (D-021, D-030).
/// </summary>
public sealed class AttendanceModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/attendance")
            .WithTags("Attendance")
            .RequireAuthorization();

        group.MapPost("/{postId:int}/request", async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new RequestAttendanceCommand(PostId.From(postId)), cancellationToken);
            if (result.IsSuccess)
                return Results.Ok();

            return IsStateConflict(result.Error) ? Results.Conflict("Already requested") : result.ToProblem();
        });

        group.MapPost("/{postId:int}/accept/{userId}", async (int postId, string userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new AcceptAttendanceCommand(PostId.From(postId), UserId.From(userId)), cancellationToken);
            return result.IsSuccess ? Results.Ok() : NotFoundOrProblem(result);
        });

        group.MapPost("/{postId:int}/reject/{userId}", async (int postId, string userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new RejectAttendanceCommand(PostId.From(postId), UserId.From(userId)), cancellationToken);
            return result.IsSuccess ? Results.Ok() : NotFoundOrProblem(result);
        });

        group.MapPost("/{postId:int}/cancel", async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CancelAttendanceCommand(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.Ok() : NotFoundOrProblem(result);
        });

        group.MapGet("/{postId:int}/check", async (int postId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new IsUserAttendingQuery(PostId.From(postId)), cancellationToken);
            return result.IsSuccess ? Results.Ok(new { attending = result.Value }) : result.ToProblem();
        });

        // Additive (plan Step 8.3): replaces the legacy GET /fetchallaccepted, /fetchallpending and /fetchaccepteddata.
        endpoints.MapGet("/api/v1/posts/{postId:int}/attendees", async (int postId, string? status, string? cursor, int? limit, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAttendeesQuery(PostId.From(postId), status, cursor, limit), cancellationToken);
            return result.IsSuccess ? Results.Ok(ToResponse(result.Value)) : result.ToProblem();
        })
            .RequireAuthorization()
            .WithTags("Attendance")
            .WithName("Attendance_GetAttendees")
            .WithSummary("Lists a post's attendees: status=accepted (default, any signed-in user) or pending (the host only), earliest request first, cursor-paged.");
    }

    private static AttendeePageResponse ToResponse(AttendeePage page) => new(
        page.Items.Select(attendee => new AttendeeResponse(
            attendee.UserId.Value,
            attendee.Status,
            attendee.RequestedAt,
            attendee.RespondedAt)).ToList(),
        page.NextCursor);

    /// <summary>A participation in the wrong state (not a concurrent save, which stays a 409 problem).</summary>
    private static bool IsStateConflict(Error error) =>
        error.Type == ErrorType.Conflict && error != UnitOfWorkErrors.ConcurrencyConflict;

    /// <summary>Today's empty 404 for a missing post or participation, a caller who is not the host, or a participation in the wrong state.</summary>
    private static IResult NotFoundOrProblem(Result result) =>
        result.Error.Type is ErrorType.NotFound or ErrorType.Forbidden || IsStateConflict(result.Error)
            ? Results.NotFound()
            : result.ToProblem();
}
