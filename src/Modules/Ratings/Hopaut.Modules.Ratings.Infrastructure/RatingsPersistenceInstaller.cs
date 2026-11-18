using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Ratings.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Ratings.Infrastructure;

public static class RatingsPersistenceInstaller
{
    /// <summary>Registers the module DbContext (same provider options as its design-time factory) and the unit of work it implements.</summary>
    public static IServiceCollection AddRatingsPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<RatingsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, RatingsModuleDbContext.SchemaName));
        services.AddScoped<IRatingsUnitOfWork>(provider => provider.GetRequiredService<RatingsModuleDbContext>());
        return services;
    }
}
