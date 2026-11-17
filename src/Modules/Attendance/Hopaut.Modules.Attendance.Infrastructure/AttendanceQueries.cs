using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

/// <summary>Serves <see cref="IAttendanceQueries"/> with no-tracking queries (the unique (PostId, UserId) index covers the post filter).</summary>
public sealed class AttendanceQueries(AttendanceModuleDbContext db) : IAttendanceQueries
{
    public Task<int> CountAcceptedAsync(PostId postId, CancellationToken cancellationToken) =>
        db.Participations
            .AsNoTracking()
            .CountAsync(p => p.PostId == postId && p.Status == AttendanceStatus.Accepted, cancellationToken);
}
