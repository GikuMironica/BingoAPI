namespace Hopaut.Modules.Users.Application.Authorization;

/// <summary>Resource-based policies of the Users module (plan Step 6.9), registered by <c>AddUsersModule</c>.</summary>
public static class UserProfilePolicies
{
    /// <summary>The caller owns the profile, or is an administrator (the legacy <c>IsProfileOwnerOrAdminAsync</c> rule).</summary>
    public const string MustOwnProfile = "users.profile.owner";
}
