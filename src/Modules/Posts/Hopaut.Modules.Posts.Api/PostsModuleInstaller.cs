using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;
using Hopaut.Modules.Posts.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Posts.Api;

public static class PostsModuleInstaller
{
    public static IServiceCollection AddPostsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddPostsPersistence(connectionString);

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IUserReputationProvider, MediatRUserReputationProvider>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<GetNearbyPostsQuery>());

        return services;
    }

    public static IEndpointRouteBuilder MapPostsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new PostsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
