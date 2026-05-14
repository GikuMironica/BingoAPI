using Hopaut.Modules.BugReports.Application;
using Hopaut.Modules.BugReports.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.BugReports.Infrastructure;

public sealed class BugReportRepository : IBugReportRepository
{
    private readonly BugReportsModuleDbContext _db;

    public BugReportRepository(BugReportsModuleDbContext db) => _db = db;

    public Task<Bug?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Bugs.Include(b => b.Screenshots).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<List<Bug>> GetAllAsync(CancellationToken ct = default)
        => _db.Bugs.Include(b => b.Screenshots).OrderByDescending(b => b.Timestamp).ToListAsync(ct);

    public async Task AddAsync(Bug bug, CancellationToken ct = default)
        => await _db.Bugs.AddAsync(bug, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
