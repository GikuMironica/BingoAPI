using Hopaut.Modules.Moderation.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Moderation.Infrastructure;

public class ModerationModuleDbContext : DbContext
{
    public const string SchemaName = "moderation";

    public ModerationModuleDbContext(DbContextOptions<ModerationModuleDbContext> options) : base(options) { }

    public DbSet<PostReport> PostReports => Set<PostReport>();
    public DbSet<UserReport> UserReports => Set<UserReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<PostReport>(entity =>
        {
            entity.ToTable("post_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ReporterId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ReportedHostId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PostId);
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.ToTable("user_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ReporterId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ReportedUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.Status);
        });
    }
}
