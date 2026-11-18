using Hopaut.Modules.Identity.Domain;

namespace Hopaut.Modules.Identity.Application;

/// <summary>
/// Generates JWT + refresh token pairs for authenticated users.
/// </summary>
public interface IJwtTokenGenerator
{
    Task<AuthResult> GenerateForUserAsync(AppUser user);
}
