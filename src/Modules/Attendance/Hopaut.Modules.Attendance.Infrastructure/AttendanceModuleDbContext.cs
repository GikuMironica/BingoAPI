using Hopaut.Modules.Attendance.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Attendance.Infrastructure;

public class AttendanceModuleDbContext : DbContext
{
    public const string SchemaName = "attendance";

    public AttendanceModuleDbContext(DbContextOptions<AttendanceModuleDbContext> options) : base(options) { }

    public DbSet<Participation> Participations => Set<Participation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.ToTable("participations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });
    }
}
