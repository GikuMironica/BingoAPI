using MediatR;

namespace Hopaut.Modules.Identity.Application.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<AuthResult>;
