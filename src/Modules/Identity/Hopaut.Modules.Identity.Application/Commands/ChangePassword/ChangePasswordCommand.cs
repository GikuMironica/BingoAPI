using MediatR;

namespace Hopaut.Modules.Identity.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword) : IRequest<AuthResult>;
