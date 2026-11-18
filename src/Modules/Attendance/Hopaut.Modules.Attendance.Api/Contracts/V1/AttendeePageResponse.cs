namespace Hopaut.Modules.Attendance.Api.Contracts.V1;

/// <summary>One attendee of a post: <c>status</c> is <c>accepted</c> or <c>pending</c>; times are ISO-8601 UTC.</summary>
public sealed record AttendeeResponse(string UserId, string Status, DateTimeOffset RequestedAt, DateTimeOffset? RespondedAt);

/// <summary>One page of attendees; <c>nextCursor</c> is null on the last page.</summary>
public sealed record AttendeePageResponse(IReadOnlyList<AttendeeResponse> Items, string? NextCursor);
