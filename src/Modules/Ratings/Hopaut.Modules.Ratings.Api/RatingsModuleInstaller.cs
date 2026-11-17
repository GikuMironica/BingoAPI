using FluentValidation;
using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Ratings.Api;

/// <summary>Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class RatingsModuleInstaller
{
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddRatingsPersistence(connectionString);

        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IUserReputationRepository, UserReputationRepository>();

        services.AddValidatorsFromAssemblyContaining<IRatingsCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapRatingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new RatingsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
