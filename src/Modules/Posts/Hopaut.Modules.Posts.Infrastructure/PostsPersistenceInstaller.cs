using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Posts.Infrastructure;

public static class PostsPersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddPostsPersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<PostsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, PostsModuleDbContext.SchemaName, PostsModuleDbContext.ConfigureNpgsql));
}
