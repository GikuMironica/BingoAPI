using Hopaut.SharedKernel;

namespace Hopaut.Modules.Users.Domain;

/// <summary>Expected failures of the UserProfile aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class UserProfileErrors
{
    public static Error NotFound(string identityUserId) =>
        Error.NotFound("users.profile.not_found", $"No profile exists for user {identityUserId}.");

    public static Error NotOwner(string identityUserId) =>
        Error.Forbidden("users.profile.not_owner", $"Only user {identityUserId} or an administrator can change this profile.");
}
