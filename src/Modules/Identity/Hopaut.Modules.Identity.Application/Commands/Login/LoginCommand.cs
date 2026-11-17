using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<AuthResult>>, IIdentityCommand;
