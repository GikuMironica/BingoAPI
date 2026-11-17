using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string Token, string RefreshToken) : ICommand<Result<AuthResult>>, IIdentityCommand;
