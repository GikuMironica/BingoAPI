using MediatR;

namespace Hopaut.Modules.Users.Application.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(string IdentityUserId) : IRequest<UserProfileDto?>;

public sealed record UserProfileDto(
    string IdentityUserId,
    string FirstName,
    string LastName,
    string ProfilePicture,
    string Description,
    long RegistrationTimeStamp);
