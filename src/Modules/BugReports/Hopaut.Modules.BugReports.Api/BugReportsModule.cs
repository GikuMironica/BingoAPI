using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.BugReports.Api;

public sealed class BugReportsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/bugs")
            .WithTags("BugReports")
            .RequireAuthorization();

        group.MapPost("/", async (CreateBugReportCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/v1/bugs/{id}", new { id });
        });
    }
}
