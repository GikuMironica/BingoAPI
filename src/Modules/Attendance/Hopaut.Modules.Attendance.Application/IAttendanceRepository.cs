using Hopaut.Modules.Attendance.Domain;

namespace Hopaut.Modules.Attendance.Application;

public interface IAttendanceRepository
{
    Task<Participation?> GetAsync(int postId, string userId, CancellationToken ct = default);
    Task<List<Participation>> GetByPostAsync(int postId, CancellationToken ct = default);
    Task<List<Participation>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<int> GetAcceptedCountAsync(int postId, CancellationToken ct = default);
    Task<bool> IsUserAttendingAsync(int postId, string userId, CancellationToken ct = default);
    Task AddAsync(Participation participation, CancellationToken ct = default);
    Task DeleteByPostAsync(int postId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
