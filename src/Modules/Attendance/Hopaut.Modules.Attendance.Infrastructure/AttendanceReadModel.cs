using Hopaut.Modules.Attendance.Application.Queries;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

/// <summary>
/// Serves <see cref="IAttendanceReadModel"/> with no-tracking queries. Lists page in SQL with a keyset row comparison, which
/// LINQ cannot express on value-converted IDs (decision D-029).
/// </summary>
public sealed class AttendanceReadModel(AttendanceModuleDbContext db) : IAttendanceReadModel
{
    public Task<bool> IsAcceptedAsync(PostId postId, UserId userId, CancellationToken ct) =>
        db.Participations
            .AsNoTracking()
            .AnyAsync(p => p.PostId == postId && p.UserId == userId && p.Status == AttendanceStatus.Accepted, ct);

    // TODO: add index — CREATE INDEX IX_participations_PostId_Status_RequestedAt_Id ON attendance.participations ("PostId", "Status", "RequestedAt", "Id");
    public async Task<IReadOnlyList<Attendee>> GetAttendeesAsync(
        PostId postId, AttendanceStatus status, AttendeeKeyset? after, int count, CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<AttendeeRow>($"""
            SELECT p."Id", p."UserId", p."Status", p."RequestedAt", p."RespondedAt"
            FROM attendance.participations AS p
            WHERE p."PostId" = {postId.Value}
              AND p."Status" = {(int)status}
              AND ({after is null} OR (p."RequestedAt", p."Id") > ({after?.RequestedAt ?? DateTimeOffset.UnixEpoch}, {after?.ParticipationId.Value ?? 0}))
            ORDER BY p."RequestedAt", p."Id"
            LIMIT {count}
            """).ToListAsync(ct);

        return rows.ConvertAll(row => new Attendee(
            ParticipationId.From(row.Id), UserId.From(row.UserId), AttendeeLists.StatusName((AttendanceStatus)row.Status), row.RequestedAt, row.RespondedAt));
    }

    private sealed class AttendeeRow
    {
        public int Id { get; init; }

        public string UserId { get; init; } = string.Empty;

        public int Status { get; init; }

        public DateTimeOffset RequestedAt { get; init; }

        public DateTimeOffset? RespondedAt { get; init; }
    }
}
