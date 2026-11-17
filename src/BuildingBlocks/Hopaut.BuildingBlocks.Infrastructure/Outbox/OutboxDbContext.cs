using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Transitional shared outbox (schema <c>messaging</c>). Replaced by per-module outboxes in plan Step 9.1.
/// </summary>
public class OutboxDbContext : DbContext
{
    public const string SchemaName = "messaging";

    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}

public sealed class OutboxDesignTimeDbContextFactory() : ModuleDesignTimeDbContextFactory<OutboxDbContext>(OutboxDbContext.SchemaName)
{
    protected override OutboxDbContext Create(DbContextOptions<OutboxDbContext> options) => new(options);
}
