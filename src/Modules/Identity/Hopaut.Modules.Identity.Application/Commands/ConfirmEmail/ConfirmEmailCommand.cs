using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(string UserId, string Token) : ICommand<Result<AuthResult>>, IIdentityCommand;
