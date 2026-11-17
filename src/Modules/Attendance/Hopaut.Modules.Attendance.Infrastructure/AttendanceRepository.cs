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

    public Task<List<Participation>> GetByPostAsync(PostId postId, CancellationToken ct = default)
        => db.Participations.Where(p => p.PostId == postId).ToListAsync(ct);

    public Task<List<Participation>> GetByUserAsync(UserId userId, CancellationToken ct = default)
        => db.Participations.Where(p => p.UserId == userId).ToListAsync(ct);

    public Task<int> GetAcceptedCountAsync(PostId postId, CancellationToken ct = default)
        => db.Participations.CountAsync(p => p.PostId == postId && p.Status == AttendanceStatus.Accepted, ct);

    public Task<bool> IsUserAttendingAsync(PostId postId, UserId userId, CancellationToken ct = default)
        => db.Participations.AnyAsync(p => p.PostId == postId && p.UserId == userId && p.Status == AttendanceStatus.Accepted, ct);

    public void Add(Participation participation) => db.Participations.Add(participation);

    // TODO (plan Step 9.3): bypasses the unit of work and domain events; the post-deleted consumer replaces it.
    public async Task DeleteByPostAsync(PostId postId, CancellationToken ct = default)
        => await db.Participations.Where(p => p.PostId == postId).ExecuteDeleteAsync(ct);
}
