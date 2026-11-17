using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Application;

/// <summary>Participation aggregate store. Changes are saved by the Attendance unit of work, never by the repository.</summary>
public interface IAttendanceRepository
{
    /// <summary>Next ID from the participations sequence, assigned before the participation is created.</summary>
    Task<ParticipationId> NextIdAsync(CancellationToken ct = default);
    Task<Participation?> GetAsync(PostId postId, UserId userId, CancellationToken ct = default);
    Task<List<Participation>> GetByPostAsync(PostId postId, CancellationToken ct = default);
    Task<List<Participation>> GetByUserAsync(UserId userId, CancellationToken ct = default);
    Task<int> GetAcceptedCountAsync(PostId postId, CancellationToken ct = default);
    Task<bool> IsUserAttendingAsync(PostId postId, UserId userId, CancellationToken ct = default);
    void Add(Participation participation);
    Task DeleteByPostAsync(PostId postId, CancellationToken ct = default);
}
