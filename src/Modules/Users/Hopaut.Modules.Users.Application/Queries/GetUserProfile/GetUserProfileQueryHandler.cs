using Hopaut.Modules.Users.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler(IUserProfileRepository profiles) : IQueryHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    public async ValueTask<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null)
            return UserProfileErrors.NotFound(request.IdentityUserId);

        return new UserProfileDto(
            profile.IdentityUserId,
            profile.FirstName,
            profile.LastName,
            profile.ProfilePicture,
            profile.Description,
            profile.RegistrationTimeStamp);
    }
}
