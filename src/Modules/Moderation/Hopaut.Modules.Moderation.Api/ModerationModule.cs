using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;
using Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;
using Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;
using Hopaut.Modules.Moderation.Application.Queries.GetOpenPostReports;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Moderation.Api;

public sealed class ModerationModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/moderation")
            .WithTags("Moderation")
            .RequireAuthorization();

        group.MapPost("/reports/post", async (CreatePostReportCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/v1/moderation/reports/post/{id.Value}", new { id = id.Value });
        });

        group.MapPost("/reports/user", async (CreateUserReportCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/v1/moderation/reports/user/{id.Value}", new { id = id.Value });
        });

        group.MapGet("/reports/post/open", async (ISender sender) =>
        {
            var result = await sender.Send(new GetOpenPostReportsQuery());
            return Results.Ok(result);
        });

        group.MapPost("/reports/post/{id:int}/resolve", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new ResolvePostReportCommand(PostReportId.From(id)));
            return result ? Results.Ok() : Results.NotFound();
        });
    }
}
