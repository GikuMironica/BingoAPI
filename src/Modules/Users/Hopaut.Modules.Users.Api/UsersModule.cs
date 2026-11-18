using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Users.Application.Commands.UpdateProfile;
using Hopaut.Modules.Users.Application.Commands.UpdateProfilePicture;
using Hopaut.Modules.Users.Application.Queries.GetUserProfile;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Users.Api;

/// <summary>Results map back to today's responses until the Users endpoints are synchronised in plan Step 10.3 (D-007).</summary>
public sealed class UsersModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{userId}", async (string userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUserProfileQuery(userId), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        });

        group.MapPut("/{userId}/profile", async (string userId, UpdateProfileRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateProfileCommand(userId, request.FirstName, request.LastName, request.Description), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        });

        group.MapPut("/{userId}/profile-picture", async (string userId, UpdateProfilePictureRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateProfilePictureCommand(userId, request.PictureUrl), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        });
    }
}

public sealed record UpdateProfileRequest(string FirstName, string LastName, string Description);
public sealed record UpdateProfilePictureRequest(string PictureUrl);
