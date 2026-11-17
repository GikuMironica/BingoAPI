using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Moderation.Application;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Moderation.Infrastructure;

public class ModerationModuleDbContext : ModuleDbContext, IModerationUnitOfWork
{
    public const string SchemaName = "moderation";

    /// <summary>Source of post-report IDs; read before a post report is created.</summary>
    public const string PostReportIdSequence = "post_report_ids";

    /// <summary>Source of user-report IDs; read before a user report is created.</summary>
    public const string UserReportIdSequence = "user_report_ids";

    public ModerationModuleDbContext(DbContextOptions<ModerationModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<PostReport> PostReports => Set<PostReport>();
    public DbSet<UserReport> UserReports => Set<UserReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.HasSequence<int>(PostReportIdSequence);
        modelBuilder.HasSequence<int>(UserReportIdSequence);

        modelBuilder.Entity<PostReport>(entity =>
        {
            entity.ToTable("post_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<PostReportId, int>>().ValueGeneratedNever();
            entity.Property(e => e.ReporterId).HasConversion<StronglyTypedIdConverter<UserId, string>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.ReportedHostId).HasConversion<StronglyTypedIdConverter<UserId, string>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PostId);
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.ToTable("user_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<UserReportId, int>>().ValueGeneratedNever();
            entity.Property(e => e.ReporterId).HasConversion<StronglyTypedIdConverter<UserId, string>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.ReportedUserId).HasConversion<StronglyTypedIdConverter<UserId, string>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.Status);
        });
    }
}
