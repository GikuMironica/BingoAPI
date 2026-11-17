using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand<Result<AuthResult>>, IIdentityCommand;
