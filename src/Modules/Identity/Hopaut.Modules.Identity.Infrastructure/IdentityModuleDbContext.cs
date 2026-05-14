using Hopaut.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Identity.Infrastructure;

/// <summary>
/// Identity module's own DbContext with schema "identity".
/// Owns ASP.NET Identity tables + RefreshTokens.
/// </summary>
public class IdentityModuleDbContext : IdentityDbContext<AppUser>
{
    public const string SchemaName = "identity";

    public IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options)
        : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Token);
            entity.Property(e => e.Token).ValueGeneratedOnAdd();

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
