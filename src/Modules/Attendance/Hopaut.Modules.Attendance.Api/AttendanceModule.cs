using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;
using Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Attendance.Api;

public sealed class AttendanceModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/attendance")
            .WithTags("Attendance")
            .RequireAuthorization();

        group.MapPost("/{postId:int}/request", async (int postId, HttpContext context, ISender sender) =>
        {
            var userId = context.User.FindFirst("id")?.Value ?? "";
            var result = await sender.Send(new RequestAttendanceCommand(postId, userId));
            return result ? Results.Ok() : Results.Conflict("Already requested");
        });

        group.MapPost("/{postId:int}/accept/{userId}", async (int postId, string userId, HttpContext context, ISender sender) =>
        {
            var ownerId = context.User.FindFirst("id")?.Value ?? "";
            var result = await sender.Send(new AcceptAttendanceCommand(postId, userId, ownerId));
            return result ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/{postId:int}/reject/{userId}", async (int postId, string userId, HttpContext context, ISender sender) =>
        {
            var ownerId = context.User.FindFirst("id")?.Value ?? "";
            var result = await sender.Send(new RejectAttendanceCommand(postId, userId, ownerId));
            return result ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/{postId:int}/cancel", async (int postId, HttpContext context, ISender sender) =>
        {
            var userId = context.User.FindFirst("id")?.Value ?? "";
            var result = await sender.Send(new CancelAttendanceCommand(postId, userId));
            return result ? Results.Ok() : Results.NotFound();
        });

        group.MapGet("/{postId:int}/check", async (int postId, HttpContext context, ISender sender) =>
        {
            var userId = context.User.FindFirst("id")?.Value ?? "";
            var attending = await sender.Send(new IsUserAttendingQuery(postId, userId));
            return Results.Ok(new { attending });
        });
    }
}
