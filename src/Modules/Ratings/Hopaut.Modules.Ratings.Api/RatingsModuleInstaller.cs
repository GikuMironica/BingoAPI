using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Application.Commands.CreateRating;
using Hopaut.Modules.Ratings.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Ratings.Api;

public static class RatingsModuleInstaller
{
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<RatingsModuleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", RatingsModuleDbContext.SchemaName)));

        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IUserReputationRepository, UserReputationRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateRatingCommand>());

        return services;
    }

    public static IEndpointRouteBuilder MapRatingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new RatingsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
