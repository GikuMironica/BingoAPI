using Hopaut.BuildingBlocks.Api;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.BugReports.Api;

/// <summary>Results map back to today's responses until the bug-report endpoints are synchronised in plan Step 10.8 (D-007).</summary>
public sealed class BugReportsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/bugs")
            .WithTags("BugReports")
            .RequireAuthorization();

        group.MapPost("/", async (CreateBugReportCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/v1/bugs/{result.Value.Value}", new { id = result.Value.Value })
                : result.ToProblem();
        });
    }
}
