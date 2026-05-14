using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

public sealed class AttendanceRepository : IAttendanceRepository
{
    private readonly AttendanceModuleDbContext _db;

    public AttendanceRepository(AttendanceModuleDbContext db) => _db = db;

    public Task<Participation?> GetAsync(int postId, string userId, CancellationToken ct = default)
        => _db.Participations.FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId, ct);

    public Task<List<Participation>> GetByPostAsync(int postId, CancellationToken ct = default)
        => _db.Participations.Where(p => p.PostId == postId).ToListAsync(ct);

    public Task<List<Participation>> GetByUserAsync(string userId, CancellationToken ct = default)
        => _db.Participations.Where(p => p.UserId == userId).ToListAsync(ct);

    public Task<int> GetAcceptedCountAsync(int postId, CancellationToken ct = default)
        => _db.Participations.CountAsync(p => p.PostId == postId && p.Status == AttendanceStatus.Accepted, ct);

    public Task<bool> IsUserAttendingAsync(int postId, string userId, CancellationToken ct = default)
        => _db.Participations.AnyAsync(p => p.PostId == postId && p.UserId == userId && p.Status == AttendanceStatus.Accepted, ct);

    public async Task AddAsync(Participation participation, CancellationToken ct = default)
        => await _db.Participations.AddAsync(participation, ct);

    public async Task DeleteByPostAsync(int postId, CancellationToken ct = default)
        => await _db.Participations.Where(p => p.PostId == postId).ExecuteDeleteAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
