using Hopaut.Modules.BugReports.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.BugReports.Application;

/// <summary>Bug aggregate store. Changes are saved by the BugReports unit of work, never by the repository.</summary>
public interface IBugReportRepository
{
    /// <summary>Next ID from the bugs sequence, assigned before the bug report is created.</summary>
    Task<BugReportId> NextIdAsync(CancellationToken ct = default);
    Task<Bug?> GetByIdAsync(BugReportId id, CancellationToken ct = default);
    Task<List<Bug>> GetAllAsync(CancellationToken ct = default);
    void Add(Bug bug);
}
