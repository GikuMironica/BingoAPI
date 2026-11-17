namespace Hopaut.BuildingBlocks.Application.Security;

/// <summary>Role names carried as role claims in access tokens and seeded at startup.</summary>
public static class HopautRoles
{
    public const string User = "User";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";

    public static IReadOnlyList<string> All { get; } = [Admin, User, SuperAdmin];
}

/// <summary>Authorization policies registered by the Host.</summary>
public static class HopautPolicies
{
    /// <summary>Administrators: the <see cref="HopautRoles.Admin"/> or <see cref="HopautRoles.SuperAdmin"/> role.</summary>
    public const string Admin = "hopaut.admin";
}
