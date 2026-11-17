using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;
using Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Attendance.Api;

/// <summary>Results map back to today's responses until the Attendance endpoints are synchronised in plan Step 10.5 (D-007).</summary>
public sealed class AttendanceModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/attendance")
            .WithTags("Attendance")
            .RequireAuthorization();

        group.MapPost("/{postId:int}/request", async (int postId, ICurrentUser currentUser, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new RequestAttendanceCommand(PostId.From(postId), currentUser.Id), cancellationToken);
            return result.IsSuccess ? Results.Ok() : Results.Conflict("Already requested");
        });

        group.MapPost("/{postId:int}/accept/{userId}", async (int postId, string userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new AcceptAttendanceCommand(PostId.From(postId), UserId.From(userId)), cancellationToken);
            return result.IsSuccess ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/{postId:int}/reject/{userId}", async (int postId, string userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new RejectAttendanceCommand(PostId.From(postId), UserId.From(userId)), cancellationToken);
            return result.IsSuccess ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/{postId:int}/cancel", async (int postId, ICurrentUser currentUser, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CancelAttendanceCommand(PostId.From(postId), currentUser.Id), cancellationToken);
            return result.IsSuccess ? Results.Ok() : Results.NotFound();
        });

        group.MapGet("/{postId:int}/check", async (int postId, ICurrentUser currentUser, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new IsUserAttendingQuery(PostId.From(postId), currentUser.Id), cancellationToken);
            return result.IsSuccess ? Results.Ok(new { attending = result.Value }) : result.ToProblem();
        });
    }
}
