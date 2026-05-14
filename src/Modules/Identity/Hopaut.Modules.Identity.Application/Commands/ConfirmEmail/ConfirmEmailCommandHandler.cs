using Hopaut.Modules.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Hopaut.Modules.Identity.Application.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, AuthResult>
{
    private readonly UserManager<AppUser> _userManager;

    public ConfirmEmailCommandHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<AuthResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return AuthResult.Fail(FailReason.UserNotFound, "User not found");

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded
            ? AuthResult.OkNoToken(user.Id)
            : AuthResult.Fail(result.Errors.Select(e => e.Description).ToArray());
    }
}
