using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

public class AttendanceModuleDbContext : ModuleDbContext, IAttendanceUnitOfWork
{
    public const string SchemaName = "attendance";

    /// <summary>Source of participation IDs; <see cref="IAttendanceRepository.NextIdAsync"/> reads it before a request is created.</summary>
    public const string ParticipationIdSequence = "participation_ids";

    public AttendanceModuleDbContext(DbContextOptions<AttendanceModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<Participation> Participations => Set<Participation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.HasSequence<int>(ParticipationIdSequence);

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.ToTable("participations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<ParticipationId, int>>().ValueGeneratedNever();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
            entity.Property(e => e.UserId).HasConversion<StronglyTypedIdConverter<UserId, string>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });
    }
}
