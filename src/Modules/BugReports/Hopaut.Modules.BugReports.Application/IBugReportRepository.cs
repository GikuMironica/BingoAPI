using Hopaut.Modules.BugReports.Domain;

namespace Hopaut.Modules.BugReports.Application;

public interface IBugReportRepository
{
    Task<Bug?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Bug>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Bug bug, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
