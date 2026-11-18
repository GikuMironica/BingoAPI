using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfilePicture;

public sealed record UpdateProfilePictureCommand(string IdentityUserId, string PictureUrl) : ICommand<Result>, IUsersCommand;
