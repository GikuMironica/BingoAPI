namespace Hopaut.Modules.Identity.Application;

public sealed record AuthResult
{
    public bool Success { get; init; }
    public string? UserId { get; init; }
    public string? Token { get; init; }
    public string? RefreshToken { get; init; }
    public FailReason? FailReason { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];

    public static AuthResult Ok(string userId, string token, string refreshToken) =>
        new() { Success = true, UserId = userId, Token = token, RefreshToken = refreshToken };

    public static AuthResult OkNoToken(string userId) =>
        new() { Success = true, UserId = userId };

    public static AuthResult Fail(params string[] errors) =>
        new() { Errors = errors };

    public static AuthResult Fail(FailReason reason, params string[] errors) =>
        new() { FailReason = reason, Errors = errors };
}

public enum FailReason
{
    EmailNotConfirmed,
    InvalidPassword,
    TooManyInvalidAttempts,
    InvalidToken,
    UserNotFound
}
