using Hopaut.Modules.Users.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfile;

// TODO (F-010, plan Step 8.8): the route user id is not compared with the caller.
public sealed class UpdateProfileCommandHandler(IUserProfileRepository profiles) : ICommandHandler<UpdateProfileCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null)
            return UserProfileErrors.NotFound(request.IdentityUserId);

        profile.UpdateProfile(request.FirstName, request.LastName, request.Description);
        return Result.Success();
    }
}
