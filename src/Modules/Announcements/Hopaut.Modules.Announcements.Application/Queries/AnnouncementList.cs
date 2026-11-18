using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application.Queries;

/// <summary>
/// An announcement as the read endpoints show it, with wire-ready values; <paramref name="EditedAt"/> is null until the
/// message is edited.
/// </summary>
public sealed record AnnouncementDto(AnnouncementId Id, PostId PostId, string Message, DateTimeOffset CreatedAt, DateTimeOffset? EditedAt);

/// <summary>One page of a post's announcements and the cursor of the next page (null on the last page), rest-api-standards §6.</summary>
public sealed record AnnouncementPage(IReadOnlyList<AnnouncementDto> Items, string? NextCursor);

/// <summary>
/// Keyset position in a post's announcements, newest first (creation time then ID, both descending): the last
/// announcement already returned.
/// </summary>
public sealed record AnnouncementKeyset(DateTimeOffset CreatedAt, AnnouncementId AnnouncementId)
{
    public static AnnouncementKeyset Of(AnnouncementDto announcement) => new(announcement.CreatedAt, announcement.Id);

    public Cursor Encode() => KeysetPosition.Encode(CreatedAt, AnnouncementId.Value);

    public static Result<AnnouncementKeyset> Decode(string? value)
    {
        var position = KeysetPosition.Decode(value);
        return position.IsSuccess
            ? new AnnouncementKeyset(position.Value.Time, AnnouncementId.From(position.Value.Id))
            : position.Error;
    }
}
