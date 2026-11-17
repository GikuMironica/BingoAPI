using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

public sealed class AttendanceRepository(AttendanceModuleDbContext db) : IAttendanceRepository
{
    public async Task<ParticipationId> NextIdAsync(CancellationToken ct = default) =>
        new(await db.Database.NextSequenceValueAsync(AttendanceModuleDbContext.SchemaName, AttendanceModuleDbContext.ParticipationIdSequence, ct));

    public Task<Participation?> GetAsync(PostId postId, UserId userId, CancellationToken ct = default)
        => db.Participations.FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId, ct);

    // The unique (PostId, UserId) index serves the post filter.
    public Task<int> CountAcceptedAsync(PostId postId, CancellationToken ct = default)
        => db.Participations.CountAsync(p => p.PostId == postId && p.Status == AttendanceStatus.Accepted, ct);

    public void Add(Participation participation) => db.Participations.Add(participation);
}
