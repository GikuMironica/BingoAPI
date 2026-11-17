using FluentValidation;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Posts.Api;

/// <summary>Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class PostsModuleInstaller
{
    public static IServiceCollection AddPostsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddPostsPersistence(connectionString);

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IUserReputationProvider, RatingsUserReputationProvider>();

        services.AddValidatorsFromAssemblyContaining<IPostsCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapPostsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new PostsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
