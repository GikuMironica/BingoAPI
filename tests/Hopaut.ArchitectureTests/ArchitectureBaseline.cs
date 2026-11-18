namespace Hopaut.ArchitectureTests;

public sealed record BaselineEntry(string Violation, string FindingId);

/// <summary>
/// Known architecture violations at the 2026-11-17 audit (decision D-002 in TODO.md). Every entry names the open
/// finding that fixes it (F-020 → 8.7/8.9, F-024/F-050/F-053 → 8.9). F-023 and F-052 were fixed in 6.6; the Posts entries
/// went in 8.2 and the Ratings entries in 8.4.
/// Entries may only be removed: a new violation must be fixed, never added here.
/// </summary>
public static class ArchitectureBaseline
{
    public static readonly IReadOnlyList<BaselineEntry> Entries =
    [
        // ARCH-DOMAIN-COLLECTION
        new("ARCH-DOMAIN-COLLECTION: Hopaut.Modules.BugReports.Domain.Bug.Screenshots", "F-020"),
        // ARCH-DOMAIN-SETTER
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.BugReports.Domain.BugScreenshot.Bug", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.BugReports.Domain.BugScreenshot.BugId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.BugReports.Domain.BugScreenshot.Id", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.BugReports.Domain.BugScreenshot.Url", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.AppUser.Description", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.AppUser.FirstName", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.AppUser.LastName", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.AppUser.ProfilePicture", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.AppUser.RegistrationTimeStamp", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.CreationDate", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.ExpiryDate", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.Invalidated", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.JwtId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.Token", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.Used", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.User", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Identity.Domain.RefreshToken.UserId", "F-020"),
        // ARCH-LAYER-APPLICATION
        new("ARCH-LAYER-APPLICATION: Hopaut.Modules.Identity.Application -> Microsoft.AspNetCore", "F-053"),
        // ARCH-LAYER-DOMAIN
        new("ARCH-LAYER-DOMAIN: Hopaut.Modules.Identity.Domain -> Microsoft.AspNetCore", "F-024"),
        new("ARCH-LAYER-DOMAIN: Hopaut.Modules.Identity.Domain -> System.ComponentModel.DataAnnotations", "F-050"),
    ];

    public static IEnumerable<BaselineEntry> ForRule(string ruleId) =>
        Entries.Where(e => e.Violation.StartsWith(ruleId + ":", StringComparison.Ordinal));
}
