using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Posts.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Posts.Infrastructure;

public static class PostsPersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddPostsPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<PostsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, PostsModuleDbContext.SchemaName, PostsModuleDbContext.ConfigureNpgsql));
        services.AddScoped<IPostsUnitOfWork>(provider => provider.GetRequiredService<PostsModuleDbContext>());
        return services;
    }
}
