using FluentValidation;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Authorization;
using Hopaut.Modules.Posts.Application.Queries;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.Modules.Posts.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>Registers the Posts module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator; IPostHostQueries is the module's public read port (Contracts).</summary>
public static class PostsModuleInstaller
{
    public static IServiceCollection AddPostsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostsPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IPostReadModel, PostReadModel>();
        services.AddScoped<IUserReputationProvider, RatingsUserReputationProvider>();
        services.AddScoped<IPostHostQueries, PostHostQueries>();

        services.AddResourceOwnerPolicy<Post>(PostPolicies.MustOwnPost, post => post.UserId, administratorsAllowed: true);

        services.AddValidatorsFromAssemblyContaining<IPostsCommand>(includeInternalTypes: true);

        return services;
    }
}
