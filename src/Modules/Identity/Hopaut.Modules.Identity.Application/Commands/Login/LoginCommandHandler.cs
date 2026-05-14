using Hopaut.Modules.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Hopaut.Modules.Identity.Application.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IJwtTokenGenerator tokenGenerator)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is not null && !user.EmailConfirmed)
            return AuthResult.Fail(FailReason.EmailNotConfirmed, "Email not yet confirmed");

        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, true);

        if (result.IsLockedOut)
            return AuthResult.Fail(FailReason.TooManyInvalidAttempts, "Account locked out, too many invalid attempts. Try again later.");

        if (!result.Succeeded)
            return AuthResult.Fail(FailReason.InvalidPassword, "Username / Password combination is wrong");

        return await _tokenGenerator.GenerateForUserAsync(user!);
    }
}
