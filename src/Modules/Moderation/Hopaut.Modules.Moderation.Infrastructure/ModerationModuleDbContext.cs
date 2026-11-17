using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Moderation.Infrastructure;

public class ModerationModuleDbContext : ModuleDbContext
{
    public const string SchemaName = "moderation";

    public ModerationModuleDbContext(DbContextOptions<ModerationModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<PostReport> PostReports => Set<PostReport>();
    public DbSet<UserReport> UserReports => Set<UserReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<PostReport>(entity =>
        {
            entity.ToTable("post_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<PostReportId>>().ValueGeneratedOnAdd();
            entity.Property(e => e.ReporterId).HasConversion<StronglyTypedStringIdConverter<UserId>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.ReportedHostId).HasConversion<StronglyTypedStringIdConverter<UserId>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId>>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PostId);
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.ToTable("user_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<UserReportId>>().ValueGeneratedOnAdd();
            entity.Property(e => e.ReporterId).HasConversion<StronglyTypedStringIdConverter<UserId>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.ReportedUserId).HasConversion<StronglyTypedStringIdConverter<UserId>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.Status);
        });
    }
}
