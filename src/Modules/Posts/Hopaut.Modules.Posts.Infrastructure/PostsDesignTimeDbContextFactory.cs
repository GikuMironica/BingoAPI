using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Posts.Infrastructure;

public sealed class PostsDesignTimeDbContextFactory()
    : ModuleDesignTimeDbContextFactory<PostsModuleDbContext>(PostsModuleDbContext.SchemaName, PostsModuleDbContext.ConfigureNpgsql)
{
    protected override PostsModuleDbContext Create(DbContextOptions<PostsModuleDbContext> options) => new(options, DesignTimePublisher.Instance);
}
