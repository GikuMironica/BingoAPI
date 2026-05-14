using Hopaut.Modules.Ratings.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

public class RatingsModuleDbContext : DbContext
{
    public const string SchemaName = "ratings";

    public RatingsModuleDbContext(DbContextOptions<RatingsModuleDbContext> options) : base(options) { }

    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<UserReputation> UserReputations => Set<UserReputation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.ToTable("ratings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.RaterId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => new { e.RaterId, e.PostId }).IsUnique();
        });

        modelBuilder.Entity<UserReputation>(entity =>
        {
            entity.ToTable("user_reputations");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(450);
        });
    }
}
