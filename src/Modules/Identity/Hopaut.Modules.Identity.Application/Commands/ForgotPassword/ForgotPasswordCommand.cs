using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result<AuthResult>>, IIdentityCommand;
