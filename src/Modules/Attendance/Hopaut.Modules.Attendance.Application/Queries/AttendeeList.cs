using System.Globalization;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Application.Queries;

/// <summary>A participation as the attendee lists show it; <paramref name="Status"/> is the wire name (<see cref="AttendeeLists.StatusName"/>).</summary>
public sealed record Attendee(
    ParticipationId ParticipationId, UserId UserId, string Status, DateTimeOffset RequestedAt, DateTimeOffset? RespondedAt);

/// <summary>One page of attendees and the cursor of the next page (null on the last page), rest-api-standards §6.</summary>
public sealed record AttendeePage(IReadOnlyList<Attendee> Items, string? NextCursor);

/// <summary>Keyset position in a list ordered by request time then ID, both ascending: the last attendee already returned.</summary>
public sealed record AttendeeKeyset(DateTimeOffset RequestedAt, ParticipationId ParticipationId)
{
    public Cursor Encode() =>
        Cursor.Encode(string.Create(CultureInfo.InvariantCulture, $"{RequestedAt.UtcTicks}:{ParticipationId.Value}"));

    public static Result<AttendeeKeyset> Decode(string? value)
    {
        var cursor = Cursor.Create(value);
        if (cursor.IsFailure)
            return cursor.Error;

        var parts = cursor.Value.Decode().Split(':');
        return parts.Length == 2
            && long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
            && ticks <= DateTimeOffset.MaxValue.UtcTicks
            && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            && id > 0
                ? new AttendeeKeyset(new DateTimeOffset(ticks, TimeSpan.Zero), ParticipationId.From(id))
                : PagingErrors.InvalidCursor;
    }
}

/// <summary>The status filter of the attendee lists and the wire names of participation statuses.</summary>
public static class AttendeeLists
{
    public static readonly Error UnknownStatus =
        Error.Validation("attendance.attendee.unknown_list_status", "The status must be \"accepted\" or \"pending\".");

    /// <summary>Parses the wire value of the list filter; absent means accepted.</summary>
    public static Result<AttendanceStatus> ParseStatus(string? status) => status?.ToUpperInvariant() switch
    {
        null or "ACCEPTED" => AttendanceStatus.Accepted,
        "PENDING" => AttendanceStatus.Pending,
        _ => UnknownStatus,
    };

    /// <summary>The wire name of a participation status: <c>pending</c>, <c>accepted</c>, <c>rejected</c> or <c>cancelled</c>.</summary>
    public static string StatusName(AttendanceStatus status) => status.ToString().ToLowerInvariant();
}
