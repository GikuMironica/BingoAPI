using Microsoft.EntityFrameworkCore;

namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Lightweight DbContext for the outbox table only.
/// Used by OutboxPublisher to poll and mark messages as processed.
/// </summary>
public class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
