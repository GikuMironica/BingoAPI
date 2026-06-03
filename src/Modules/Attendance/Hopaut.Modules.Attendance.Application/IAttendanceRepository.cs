using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Application;

public interface IAttendanceRepository
{
    Task<Participation?> GetAsync(PostId postId, UserId userId, CancellationToken ct = default);
    Task<List<Participation>> GetByPostAsync(PostId postId, CancellationToken ct = default);
    Task<List<Participation>> GetByUserAsync(UserId userId, CancellationToken ct = default);
    Task<int> GetAcceptedCountAsync(PostId postId, CancellationToken ct = default);
    Task<bool> IsUserAttendingAsync(PostId postId, UserId userId, CancellationToken ct = default);
    Task AddAsync(Participation participation, CancellationToken ct = default);
    Task DeleteByPostAsync(PostId postId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
