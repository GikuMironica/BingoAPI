using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Application;

/// <summary>
/// UserReport aggregate store for the write paths; reads go through <c>IModerationReadModel</c>. Changes are saved by the
/// Moderation unit of work, never by the repository.
/// </summary>
public interface IUserReportRepository
{
    /// <summary>Next ID from the user-reports sequence, assigned before the report is created.</summary>
    Task<UserReportId> NextIdAsync(CancellationToken ct = default);

    Task<UserReport?> GetByIdAsync(UserReportId id, CancellationToken ct = default);

    /// <summary>When the reporter last reported the user in a report that is still open; null when there is none.</summary>
    Task<DateTimeOffset?> GetLatestOpenReportTimeAsync(UserId reporterId, UserId reportedUserId, CancellationToken ct = default);

    void Add(UserReport report);
}
