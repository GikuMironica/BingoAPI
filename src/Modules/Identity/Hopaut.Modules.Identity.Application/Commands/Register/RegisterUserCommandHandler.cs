using Hopaut.Modules.Identity.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Hopaut.Modules.Identity.Application.Commands.Register;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<AuthResult>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly TimeProvider _timeProvider;

    public RegisterUserCommandHandler(UserManager<AppUser> userManager, ILogger<RegisterUserCommandHandler> logger, TimeProvider timeProvider)
    {
        _userManager = userManager;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async ValueTask<Result<AuthResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return AuthResult.Fail("User with this email address exists");

        var newUser = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            // TODO: Int64 Unix seconds → DateTimeOffset (finding F-019, plan Step 8.9).
            RegistrationTimeStamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds()
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
