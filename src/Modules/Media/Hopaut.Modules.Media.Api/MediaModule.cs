using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Media.Application.Commands.UploadImages;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Media.Api;

public sealed class MediaModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/media")
            .WithTags("Media")
            .RequireAuthorization();

        group.MapPost("/upload", async (HttpRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var bucketPath = form["bucketPath"].ToString();
            var userId = request.HttpContext.User.FindFirst("id")?.Value ?? "unknown";

            var images = form.Files.Select(f => new ImageInput(
                f.OpenReadStream(),
                f.FileName,
                f.ContentType)).ToList();

            var result = await sender.Send(new UploadImagesCommand(images, bucketPath, userId), cancellationToken);

            // Today's response shapes until plan Step 10.9 (D-007).
            return result.IsSuccess
                ? Results.Ok(new { ImageKeys = result.Value })
                : Results.BadRequest(new { Error = result.Error.Description });
        }).DisableAntiforgery();
    }
}
