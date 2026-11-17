using FluentValidation;
using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Ratings.Infrastructure;

/// <summary>Registers the Ratings module's services. Handlers, domain-event handlers and the unit-of-work behavior are registered by the Host's AddMediator; IUserReputationQueries is the module's public read port (Contracts).</summary>
public static class RatingsModuleInstaller
{
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRatingsPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IUserReputationRepository, UserReputationRepository>();
        services.AddScoped<IUserReputationQueries, UserReputationQueries>();

        services.AddValidatorsFromAssemblyContaining<IRatingsCommand>(includeInternalTypes: true);

        return services;
    }
}
