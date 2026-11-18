using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Announcements.Infrastructure;

public class AnnouncementsModuleDbContext : ModuleDbContext, IAnnouncementsUnitOfWork
{
    public const string SchemaName = "announcements";

    /// <summary>Source of announcement IDs; <see cref="IAnnouncementRepository.NextIdAsync"/> reads it before an announcement is created.</summary>
    public const string AnnouncementIdSequence = "announcement_ids";

    public AnnouncementsModuleDbContext(DbContextOptions<AnnouncementsModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.HasSequence<int>(AnnouncementIdSequence);

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("announcements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<AnnouncementId, int>>().ValueGeneratedNever();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
            entity.Property(e => e.Message)
                .HasConversion<ValueObjectConverter<AnnouncementMessage, string>>()
                .IsRequired()
                .HasMaxLength(AnnouncementMessage.MaxLength);
            entity.Property(e => e.Version).IsRowVersion();
            entity.HasIndex(e => e.PostId);
        });
    }
}
