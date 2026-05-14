using Hopaut.Modules.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Hopaut.Modules.Identity.Application.Commands.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResult>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(UserManager<AppUser> userManager, ILogger<RegisterUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return AuthResult.Fail("User with this email address exists");

        var newUser = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            RegistrationTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var result = await _userManager.CreateAsync(newUser, request.Password);
        if (!result.Succeeded)
            return AuthResult.Fail(result.Errors.Select(e => e.Description).ToArray());

        await _userManager.AddToRoleAsync(newUser, "User");
        await _userManager.AddClaimAsync(newUser, new System.Security.Claims.Claim("post.add", "true"));

        var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        _logger.LogInformation("User {UserId} registered. Confirmation token generated.", newUser.Id);

        // TODO: Publish UserRegisteredIntegrationEvent to outbox for welcome email

        return AuthResult.OkNoToken(newUser.Id);
    }
}
