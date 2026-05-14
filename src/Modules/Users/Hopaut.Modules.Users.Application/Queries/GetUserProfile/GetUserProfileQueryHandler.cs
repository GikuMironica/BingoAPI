using MediatR;

namespace Hopaut.Modules.Users.Application.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IUserProfileRepository _repo;

    public GetUserProfileQueryHandler(IUserProfileRepository repo) => _repo = repo;

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repo.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null) return null;

        return new UserProfileDto(
            profile.IdentityUserId,
            profile.FirstName,
            profile.LastName,
            profile.ProfilePicture,
            profile.Description,
            profile.RegistrationTimeStamp);
    }
}
