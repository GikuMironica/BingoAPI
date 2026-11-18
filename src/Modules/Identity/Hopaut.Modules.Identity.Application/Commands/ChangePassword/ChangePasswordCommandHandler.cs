using Hopaut.Modules.Identity.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace Hopaut.Modules.Identity.Application.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Result<AuthResult>>
{
    private readonly UserManager<AppUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async ValueTask<Result<AuthResult>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return AuthResult.Fail(FailReason.UserNotFound, "User not found");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded
            ? AuthResult.OkNoToken(user.Id)
            : AuthResult.Fail(result.Errors.Select(e => e.Description).ToArray());
    }
}
