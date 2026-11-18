using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.BugReports.Application;
using Hopaut.Modules.BugReports.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.BugReports.Infrastructure;

public sealed class BugReportRepository(BugReportsModuleDbContext db) : IBugReportRepository
{
    public async Task<BugReportId> NextIdAsync(CancellationToken ct = default) =>
        new(await db.Database.NextSequenceValueAsync(BugReportsModuleDbContext.SchemaName, BugReportsModuleDbContext.BugIdSequence, ct));

    public Task<Bug?> GetByIdAsync(BugReportId id, CancellationToken ct = default)
        => db.Bugs.Include(b => b.Screenshots).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<List<Bug>> GetAllAsync(CancellationToken ct = default)
        => db.Bugs.Include(b => b.Screenshots).OrderByDescending(b => b.CreatedAt).ToListAsync(ct);

    public void Add(Bug bug) => db.Bugs.Add(bug);
}
