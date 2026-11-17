using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;
using Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;
using Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;
using Hopaut.Modules.Moderation.Application.Queries.GetOpenPostReports;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Moderation.Api;

/// <summary>Results map back to today's responses until the Moderation endpoints are synchronised in plan Step 10.8 (D-007).</summary>
public sealed class ModerationModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/moderation")
            .WithTags("Moderation")
            .RequireAuthorization();

        group.MapPost("/reports/post", async (CreatePostReportCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/v1/moderation/reports/post/{result.Value.Value}", new { id = result.Value.Value })
                : result.ToProblem();
        });

        group.MapPost("/reports/user", async (CreateUserReportCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/v1/moderation/reports/user/{result.Value.Value}", new { id = result.Value.Value })
                : result.ToProblem();
        });

        group.MapGet("/reports/post/open", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOpenPostReportsQuery(), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        }).RequireAuthorization(HopautPolicies.Admin);

        group.MapPost("/reports/post/{id:int}/resolve", async (int id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ResolvePostReportCommand(PostReportId.From(id)), cancellationToken);
            return result.IsSuccess ? Results.Ok() : Results.NotFound();
        }).RequireAuthorization(HopautPolicies.Admin);
    }
}
