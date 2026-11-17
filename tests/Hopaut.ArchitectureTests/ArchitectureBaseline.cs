namespace Hopaut.ArchitectureTests;

public sealed record BaselineEntry(string Violation, string FindingId);

/// <summary>
/// Known architecture violations at the 2026-11-17 audit (decision D-002 in TODO.md). Every entry names the open
/// finding that fixes it (F-020 → 8.2/8.4/8.7/8.9, F-024/F-050/F-053 → 8.9). F-023 and F-052 were fixed in 6.6.
/// Entries may only be removed: a new violation must be fixed, never added here.
/// </summary>
public static class ArchitectureBaseline
{
    public static readonly IReadOnlyList<BaselineEntry> Entries =
    [
        // ARCH-DOMAIN-COLLECTION
        new("ARCH-DOMAIN-COLLECTION: Hopaut.Modules.BugReports.Domain.Bug.Screenshots", "F-020"),
        new("ARCH-DOMAIN-COLLECTION: Hopaut.Modules.Posts.Domain.Post.Pictures", "F-020"),
        new("ARCH-DOMAIN-COLLECTION: Hopaut.Modules.Posts.Domain.Post.Tags", "F-020"),
        new("ARCH-DOMAIN-COLLECTION: Hopaut.Modules.Posts.Domain.Tag.Posts", "F-020"),
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
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.Currency", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.Description", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.EntrancePrice", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.EventType", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.Id", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.Post", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.PostId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.Requirements", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.Slots", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.Title", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Event.TypeSpecificData", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.Address", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.City", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.Country", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.EntityName", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.Id", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.Location", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.Post", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.PostId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.EventLocation.Region", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Picture.Id", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Picture.Post", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Picture.PostId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Picture.State", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Picture.TempKey", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Picture.Url", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.PostTag.Post", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.PostTag.PostId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.PostTag.Tag", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.PostTag.TagId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.RepeatableProperty.Id", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.RepeatableProperty.Post", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.RepeatableProperty.PostId", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Tag.Id", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Tag.Posts", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Posts.Domain.Tag.TagName", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Ratings.Domain.UserReputation.AverageRating", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Ratings.Domain.UserReputation.SumRatings", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Ratings.Domain.UserReputation.TotalRatings", "F-020"),
        new("ARCH-DOMAIN-SETTER: Hopaut.Modules.Ratings.Domain.UserReputation.UserId", "F-020"),
        // ARCH-LAYER-APPLICATION
        new("ARCH-LAYER-APPLICATION: Hopaut.Modules.Identity.Application -> Microsoft.AspNetCore", "F-053"),
        // ARCH-LAYER-DOMAIN
        new("ARCH-LAYER-DOMAIN: Hopaut.Modules.Identity.Domain -> Microsoft.AspNetCore", "F-024"),
        new("ARCH-LAYER-DOMAIN: Hopaut.Modules.Identity.Domain -> System.ComponentModel.DataAnnotations", "F-050"),
    ];

    public static IEnumerable<BaselineEntry> ForRule(string ruleId) =>
        Entries.Where(e => e.Violation.StartsWith(ruleId + ":", StringComparison.Ordinal));
}
