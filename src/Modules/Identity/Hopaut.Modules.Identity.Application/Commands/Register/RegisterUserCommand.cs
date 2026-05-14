using MediatR;

namespace Hopaut.Modules.Identity.Application.Commands.Register;

public sealed record RegisterUserCommand(string Email, string Password, string? Language = null) : IRequest<AuthResult>;
