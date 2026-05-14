using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.BuildingBlocks.Infrastructure;

/// <summary>
/// Base DbContext that dispatches domain events after SaveChanges.
/// Modules inherit from this to get automatic event dispatch.
/// </summary>
public abstract class ModuleDbContext : DbContext, IUnitOfWork
{
    protected ModuleDbContext(DbContextOptions options) : base(options) { }
}
