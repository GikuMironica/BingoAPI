using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(string IdentityUserId, string FirstName, string LastName, string Description)
    : ICommand<Result>, IUsersCommand;
