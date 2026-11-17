using Hopaut.Modules.Identity.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace Hopaut.Modules.Identity.Application.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, Result<AuthResult>>
{
    private readonly UserManager<AppUser> _userManager;

    public ResetPasswordCommandHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async ValueTask<Result<AuthResult>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return AuthResult.Fail(FailReason.UserNotFound, "User not found");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded
            ? AuthResult.OkNoToken(user.Id)
            : AuthResult.Fail(result.Errors.Select(e => e.Description).ToArray());
    }
}
