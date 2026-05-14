using MediatR;

namespace Hopaut.Modules.Identity.Application.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
