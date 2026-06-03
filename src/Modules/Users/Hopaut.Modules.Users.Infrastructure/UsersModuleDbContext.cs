using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Users.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Users.Infrastructure;

public class UsersModuleDbContext : ModuleDbContext
{
    public const string SchemaName = "users";

    public UsersModuleDbContext(DbContextOptions<UsersModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasConversion(id => id.Value, value => new UserProfileId(value));
            entity.Property(e => e.IdentityUserId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.IdentityUserId).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(200);
            entity.Property(e => e.LastName).HasMaxLength(200);
            entity.Property(e => e.ProfilePicture).HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(2000);

            // Ignore domain events collection from base class
            entity.Ignore(e => e.DomainEvents);
        });
    }
}
