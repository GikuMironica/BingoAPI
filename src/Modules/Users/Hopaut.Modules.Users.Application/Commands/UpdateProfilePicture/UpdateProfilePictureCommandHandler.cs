using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Users.Application.Authorization;
using Hopaut.Modules.Users.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandHandler(IUserProfileRepository profiles, IResourceAuthorizer authorizer)
    : ICommandHandler<UpdateProfilePictureCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null)
            return UserProfileErrors.NotFound(request.IdentityUserId);

        if (!await authorizer.IsAuthorizedAsync(profile, UserProfilePolicies.MustOwnProfile, cancellationToken))
            return UserProfileErrors.NotOwner(request.IdentityUserId);

        profile.UpdateProfilePicture(request.PictureUrl);
        return Result.Success();
    }
}
