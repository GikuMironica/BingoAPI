using MediatR;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(string IdentityUserId, string FirstName, string LastName, string Description) : IRequest<bool>;
