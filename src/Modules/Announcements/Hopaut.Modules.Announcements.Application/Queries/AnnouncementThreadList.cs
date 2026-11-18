using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application.Queries;

/// <summary>Which announcement threads to list: the posts the caller attends as an accepted attendee (inbox) or hosts (outbox).</summary>
public enum AnnouncementBox
{
    Inbox,
    Outbox,
}

/// <summary>A post that has announcements, with the newest of them (the most recently created one; edits do not reorder).</summary>
public sealed record AnnouncementThread(PostId PostId, AnnouncementDto LastAnnouncement);

/// <summary>One page of announcement threads and the cursor of the next page (null on the last page), rest-api-standards §6.</summary>
public sealed record AnnouncementThreadPage(IReadOnlyList<AnnouncementThread> Items, string? NextCursor);

/// <summary>
/// Keyset position in a thread list ordered by the creation time of each post's newest announcement, then post ID, both
/// descending: the last thread already returned.
/// </summary>
public sealed record AnnouncementThreadKeyset(DateTimeOffset LastAnnouncedAt, PostId PostId)
{
    public static AnnouncementThreadKeyset Of(AnnouncementThread thread) => new(thread.LastAnnouncement.CreatedAt, thread.PostId);

    public Cursor Encode() => KeysetPosition.Encode(LastAnnouncedAt, PostId.Value);

    public static Result<AnnouncementThreadKeyset> Decode(string? value)
    {
        var position = KeysetPosition.Decode(value);
        return position.IsSuccess
            ? new AnnouncementThreadKeyset(position.Value.Time, PostId.From(position.Value.Id))
            : position.Error;
    }
}

/// <summary>The box filter of the thread lists.</summary>
public static class AnnouncementThreads
{
    public static readonly Error UnknownBox =
        Error.Validation("announcements.thread.unknown_box", "The box must be \"inbox\" or \"outbox\".");

    /// <summary>Parses the wire value of the box filter; absent means inbox.</summary>
    public static Result<AnnouncementBox> ParseBox(string? box) => box?.ToUpperInvariant() switch
    {
        null or "INBOX" => AnnouncementBox.Inbox,
        "OUTBOX" => AnnouncementBox.Outbox,
        _ => UnknownBox,
    };
}
