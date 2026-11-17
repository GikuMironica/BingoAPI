using Hopaut.Modules.Attendance.Application.Queries;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

/// <summary>Serves <see cref="IAttendanceReadModel"/> with no-tracking queries.</summary>
public sealed class AttendanceReadModel(AttendanceModuleDbContext db) : IAttendanceReadModel
{
    public Task<bool> IsAcceptedAsync(PostId postId, UserId userId, CancellationToken ct) =>
        db.Participations
            .AsNoTracking()
            .AnyAsync(p => p.PostId == postId && p.UserId == userId && p.Status == AttendanceStatus.Accepted, ct);
}
