using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.BugReports.Domain;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.BugReports.Infrastructure;

public class BugReportsModuleDbContext : ModuleDbContext
{
    public const string SchemaName = "bug_reports";

    public BugReportsModuleDbContext(DbContextOptions<BugReportsModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<Bug> Bugs => Set<Bug>();
    public DbSet<BugScreenshot> BugScreenshots => Set<BugScreenshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<Bug>(entity =>
        {
            entity.ToTable("bugs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<BugReportId, int>>().ValueGeneratedOnAdd();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.ReporterId).HasConversion<StronglyTypedIdConverter<UserId, string>>().IsRequired().HasMaxLength(450);
            entity.HasMany(e => e.Screenshots).WithOne(s => s.Bug).HasForeignKey(s => s.BugId);
        });

        modelBuilder.Entity<BugScreenshot>(entity =>
        {
            entity.ToTable("bug_screenshots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<BugScreenshotId, int>>().ValueGeneratedOnAdd();
            entity.Property(e => e.BugId).HasConversion<StronglyTypedIdConverter<BugReportId, int>>();
        });
    }
}
