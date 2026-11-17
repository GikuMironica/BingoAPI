using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword) : ICommand<Result<AuthResult>>, IIdentityCommand;
