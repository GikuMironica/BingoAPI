using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.UnitTests;

/// <summary>Stable test data: one post hosted by <see cref="Host"/>, requested by <see cref="Guest"/>.</summary>
internal static class AttendanceTestData
{
    public static readonly DateTimeOffset Now = new(2030, 1, 9, 12, 0, 0, TimeSpan.Zero);
    public static readonly PostId Post = PostId.From(7);
    public static readonly UserId Host = UserId.From("host-1");
    public static readonly UserId Guest = UserId.From("guest-1");
    public static readonly ParticipationId Id = ParticipationId.From(31);

    /// <summary>A listed house party (the host approves attendees) ending in six hours.</summary>
    public static AttendanceTerms HouseParty(int? slots = null) =>
        new(Post, Host, IsListed: true, EndsAt: Now.AddHours(6), HostApprovesAttendees: true, Slots(slots));

    /// <summary>A listed event other than a house party (requests are accepted at once) ending in six hours.</summary>
    public static AttendanceTerms OpenEvent(int? slots = null) =>
        new(Post, Host, IsListed: true, EndsAt: Now.AddHours(6), HostApprovesAttendees: false, Slots(slots));

    /// <summary>A participation of <see cref="Guest"/> in the given state, without pending domain events.</summary>
    public static Participation ParticipationIn(AttendanceStatus status)
    {
        var participation = Participation.Request(
            Id, status == AttendanceStatus.Accepted ? OpenEvent() : HouseParty(), Guest, acceptedAttendees: 0, Now).Value;

        var moved = status switch
        {
            AttendanceStatus.Rejected => participation.Reject(Now),
            AttendanceStatus.Cancelled => participation.Cancel(Now),
            _ => Result.Success(),
        };

        moved.IsSuccess.Should().BeTrue();
        participation.ClearDomainEvents();
        return participation;
    }

    private static Capacity? Slots(int? slots) => slots is { } count ? Capacity.Create(count).Value : null;
}
