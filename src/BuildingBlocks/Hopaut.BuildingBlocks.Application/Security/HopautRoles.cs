namespace Hopaut.BuildingBlocks.Application.Security;

/// <summary>Role names carried as role claims in access tokens and seeded at startup.</summary>
public static class HopautRoles
{
    public const string User = "User";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";

    public static IReadOnlyList<string> All { get; } = [Admin, User, SuperAdmin];

    /// <summary>The roles that may moderate other users' resources.</summary>
    public static IReadOnlyList<string> Administrators { get; } = [Admin, SuperAdmin];
}

/// <summary>Authorization policies registered by the Host; resource-based policies are registered by each module.</summary>
public static class HopautPolicies
{
    /// <summary>Administrators: one of the <see cref="HopautRoles.Administrators"/> roles.</summary>
    public const string Admin = "hopaut.admin";
}
