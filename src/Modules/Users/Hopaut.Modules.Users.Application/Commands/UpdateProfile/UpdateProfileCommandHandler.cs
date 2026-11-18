using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Users.Application.Authorization;
using Hopaut.Modules.Users.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(IUserProfileRepository profiles, IResourceAuthorizer authorizer)
    : ICommandHandler<UpdateProfileCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null)
            return UserProfileErrors.NotFound(request.IdentityUserId);

        if (!await authorizer.IsAuthorizedAsync(profile, UserProfilePolicies.MustOwnProfile, cancellationToken))
            return UserProfileErrors.NotOwner(request.IdentityUserId);

        profile.UpdateProfile(request.FirstName, request.LastName, request.Description);
        return Result.Success();
    }
}
