using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application.Commands.Register;

public sealed record RegisterUserCommand(string Email, string Password, string? Language = null) : ICommand<Result<AuthResult>>, IIdentityCommand;
