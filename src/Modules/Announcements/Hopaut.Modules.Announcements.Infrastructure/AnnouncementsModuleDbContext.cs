using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Announcements.Infrastructure;

public class AnnouncementsModuleDbContext : ModuleDbContext
{
    public const string SchemaName = "announcements";

    public AnnouncementsModuleDbContext(DbContextOptions<AnnouncementsModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("announcements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<AnnouncementId>>().ValueGeneratedOnAdd();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId>>();
            entity.Property(e => e.Message).IsRequired();
            entity.HasIndex(e => e.PostId);
        });
    }
}
