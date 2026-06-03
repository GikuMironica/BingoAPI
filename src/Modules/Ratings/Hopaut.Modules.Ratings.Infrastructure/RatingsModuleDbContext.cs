using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

public class RatingsModuleDbContext : ModuleDbContext
{
    public const string SchemaName = "ratings";

    public RatingsModuleDbContext(DbContextOptions<RatingsModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<UserReputation> UserReputations => Set<UserReputation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.ToTable("ratings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<RatingId>>().ValueGeneratedOnAdd();
            entity.Property(e => e.Rate).HasConversion(
                v => v.Value,
                v => RatingValue.From(v));
            entity.Property(e => e.UserId).HasConversion<StronglyTypedStringIdConverter<UserId>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.RaterId).HasConversion<StronglyTypedStringIdConverter<UserId>>().IsRequired().HasMaxLength(450);
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId>>();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => new { e.RaterId, e.PostId }).IsUnique();
        });

        modelBuilder.Entity<UserReputation>(entity =>
        {
            entity.ToTable("user_reputations");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasConversion<StronglyTypedStringIdConverter<UserId>>().HasMaxLength(450);
        });
    }
}
