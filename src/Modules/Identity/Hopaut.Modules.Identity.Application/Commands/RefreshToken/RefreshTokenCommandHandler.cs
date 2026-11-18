using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Hopaut.Modules.Identity.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Hopaut.Modules.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<AuthResult>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepo,
        UserManager<AppUser> userManager,
        TokenValidationParameters tokenValidationParameters,
        IJwtTokenGenerator tokenGenerator,
        TimeProvider timeProvider)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _userManager = userManager;
        _tokenValidationParameters = tokenValidationParameters;
        _tokenGenerator = tokenGenerator;
        _timeProvider = timeProvider;
    }

    public async ValueTask<Result<AuthResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = GetPrincipalFromToken(request.Token);
        if (principal is null)
            return AuthResult.Fail(FailReason.InvalidToken, "Invalid Token");

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var expiryDateUnix = long.Parse(principal.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Exp).Value);
        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expiryDateUnix).UtcDateTime;

        if (expiryDate > now)
            return AuthResult.Fail(FailReason.InvalidToken, "This token hasn't expired yet");

        var jti = principal.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var storedRefreshToken = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedRefreshToken is null)
            return AuthResult.Fail(FailReason.InvalidToken, "This refresh token does not exist");

        if (now > storedRefreshToken.ExpiryDate)
            return AuthResult.Fail(FailReason.InvalidToken, "This refresh token has expired");

        if (storedRefreshToken.Invalidated)
            return AuthResult.Fail(FailReason.InvalidToken, "This refresh token has been invalidated");

        if (storedRefreshToken.Used)
            return AuthResult.Fail(FailReason.InvalidToken, "This refresh token has been used");

        if (storedRefreshToken.JwtId != jti)
            return AuthResult.Fail(FailReason.InvalidToken, "This refresh token does not match this JWT");

        _refreshTokenRepo.MarkUsed(storedRefreshToken);

        var user = await _userManager.FindByIdAsync(principal.Claims.Single(c => c.Type == "id").Value);
        if (user is null)
            return AuthResult.Fail(FailReason.UserNotFound, "User not found");

        return await _tokenGenerator.GenerateForUserAsync(user);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var validationParams = _tokenValidationParameters.Clone();
            validationParams.ValidateLifetime = false;
            var principal = tokenHandler.ValidateToken(token, validationParams, out var validatedToken);
            return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken) =>
        validatedToken is JwtSecurityToken jwtToken &&
        jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
}
