using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

public class AttendanceModuleDbContext : ModuleDbContext
{
    public const string SchemaName = "attendance";

    public AttendanceModuleDbContext(DbContextOptions<AttendanceModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<Participation> Participations => Set<Participation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.ToTable("participations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<ParticipationId>>().ValueGeneratedOnAdd();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId>>();
            entity.Property(e => e.UserId).HasConversion<StronglyTypedStringIdConverter<UserId>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });
    }
}
