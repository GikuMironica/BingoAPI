using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Users.Application.Commands.UpdateProfile;
using Hopaut.Modules.Users.Application.Commands.UpdateProfilePicture;
using Hopaut.Modules.Users.Application.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Users.Api;

public sealed class UsersModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{userId}", async (string userId, ISender sender) =>
        {
            var result = await sender.Send(new GetUserProfileQuery(userId));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapPut("/{userId}/profile", async (string userId, UpdateProfileRequest request, ISender sender) =>
        {
            var success = await sender.Send(new UpdateProfileCommand(userId, request.FirstName, request.LastName, request.Description));
            return success ? Results.NoContent() : Results.NotFound();
        });

        group.MapPut("/{userId}/profile-picture", async (string userId, UpdateProfilePictureRequest request, ISender sender) =>
        {
            var success = await sender.Send(new UpdateProfilePictureCommand(userId, request.PictureUrl));
            return success ? Results.NoContent() : Results.NotFound();
        });
    }
}

public sealed record UpdateProfileRequest(string FirstName, string LastName, string Description);
public sealed record UpdateProfilePictureRequest(string PictureUrl);
