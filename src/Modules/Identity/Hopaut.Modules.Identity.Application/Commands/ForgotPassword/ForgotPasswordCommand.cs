using MediatR;

namespace Hopaut.Modules.Identity.Application.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<AuthResult>;
