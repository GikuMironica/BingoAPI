using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Domain;

/// <summary>
/// The terms of the post a participation belongs to, read from the Posts module by the handlers: its host, whether it is
/// listed (not disabled), when it ends (null: no end), whether the host approves each attendee, and how many attendees it
/// admits (null: no limit).
/// </summary>
public sealed record AttendanceTerms(
    PostId PostId,
    UserId HostId,
    bool IsListed,
    DateTimeOffset? EndsAt,
    bool HostApprovesAttendees,
    Capacity? Slots)
{
    public bool HasEndedAt(DateTimeOffset now) => EndsAt is { } end && now >= end;

    public bool IsFull(int acceptedAttendees) => Slots is { } slots && acceptedAttendees >= slots.Value;
}
