using MediatR;

namespace Hopaut.Modules.Identity.Application.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(string UserId, string Token) : IRequest<AuthResult>;
