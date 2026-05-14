using MediatR;

namespace Hopaut.Modules.Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<AuthResult>;
