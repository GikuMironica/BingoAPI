using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.BugReports.Application;
using Hopaut.Modules.BugReports.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.BugReports.Infrastructure;

public class BugReportsModuleDbContext : ModuleDbContext, IBugReportsUnitOfWork
{
    public const string SchemaName = "bug_reports";

    /// <summary>Source of bug-report IDs; <see cref="IBugReportRepository.NextIdAsync"/> reads it before a report is created.</summary>
    public const string BugIdSequence = "bug_ids";

    public BugReportsModuleDbContext(DbContextOptions<BugReportsModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<Bug> Bugs => Set<Bug>();
    public DbSet<BugScreenshot> BugScreenshots => Set<BugScreenshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.HasSequence<int>(BugIdSequence);

        modelBuilder.Entity<Bug>(entity =>
        {
            entity.ToTable("bugs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<BugReportId, int>>().ValueGeneratedNever();
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
