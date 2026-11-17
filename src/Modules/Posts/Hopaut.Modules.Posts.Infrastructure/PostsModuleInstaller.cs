using FluentValidation;
using Hopaut.Modules.Posts.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>Registers the Posts module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class PostsModuleInstaller
{
    public static IServiceCollection AddPostsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostsPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IUserReputationProvider, RatingsUserReputationProvider>();

        services.AddValidatorsFromAssemblyContaining<IPostsCommand>(includeInternalTypes: true);

        return services;
    }
}
