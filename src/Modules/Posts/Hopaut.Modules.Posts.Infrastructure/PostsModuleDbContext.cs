using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Hopaut.Modules.Posts.Infrastructure;

public class PostsModuleDbContext : ModuleDbContext, IPostsUnitOfWork
{
    public const string SchemaName = "posts";

    /// <summary>Source of post IDs; <see cref="IPostRepository.NextIdAsync"/> reads it before a post is created.</summary>
    public const string PostIdSequence = "post_ids";

    public PostsModuleDbContext(DbContextOptions<PostsModuleDbContext> options, IPublisher publisher) : base(options, publisher) { }

    /// <summary>Provider options shared by the runtime registration and the design-time factory (PostGIS types).</summary>
    public static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) => npgsql.UseNetTopologySuite();

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventLocation> EventLocations => Set<EventLocation>();
    public DbSet<Picture> Pictures => Set<Picture>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<RepeatableProperty> RepeatableProperties => Set<RepeatableProperty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.HasSequence<int>(PostIdSequence);

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<PostId, int>>().ValueGeneratedNever();
            entity.Property(e => e.UserId).HasConversion<StronglyTypedIdConverter<UserId, string>>().IsRequired().HasMaxLength(450);
            entity.HasOne(e => e.Location).WithOne(l => l.Post).HasForeignKey<EventLocation>(l => l.PostId);
            entity.HasOne(e => e.Event).WithOne(ev => ev.Post).HasForeignKey<Event>(ev => ev.PostId);
            entity.HasOne(e => e.Repeatable).WithOne(r => r.Post).HasForeignKey<RepeatableProperty>(r => r.PostId);
            entity.HasMany(e => e.Pictures).WithOne(p => p.Post).HasForeignKey(p => p.PostId);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
            entity.Property(e => e.EventType).HasConversion<int>();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.TypeSpecificData).HasColumnType("jsonb");
            entity.HasIndex(e => e.EventType);
        });

        modelBuilder.Entity<EventLocation>(entity =>
        {
            entity.ToTable("event_locations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
            entity.Property(e => e.Location).HasColumnType("geography (point)");
            entity.HasIndex(e => e.Location).HasMethod("gist");
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.ToTable("pictures");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<PictureId, int>>().ValueGeneratedOnAdd();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
            entity.Property(e => e.State).HasConversion<int>();
            entity.HasIndex(e => new { e.State, e.PostId });
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion<StronglyTypedIdConverter<TagId, int>>().ValueGeneratedOnAdd();
            entity.Property(e => e.TagName).IsRequired();
        });

        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.ToTable("post_tags");
            entity.HasKey(e => new { e.PostId, e.TagId });
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
            entity.Property(e => e.TagId).HasConversion<StronglyTypedIdConverter<TagId, int>>();
        });

        modelBuilder.Entity<RepeatableProperty>(entity =>
        {
            entity.ToTable("repeatable_properties");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PostId).HasConversion<StronglyTypedIdConverter<PostId, int>>();
        });
    }
}
