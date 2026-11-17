using Hopaut.Modules.Users.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfilePicture;

// TODO (F-010, plan Step 8.8): the route user id is not compared with the caller.
public sealed class UpdateProfilePictureCommandHandler(IUserProfileRepository profiles) : ICommandHandler<UpdateProfilePictureCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null)
            return UserProfileErrors.NotFound(request.IdentityUserId);

        profile.UpdateProfilePicture(request.PictureUrl);
        return Result.Success();
    }
}
