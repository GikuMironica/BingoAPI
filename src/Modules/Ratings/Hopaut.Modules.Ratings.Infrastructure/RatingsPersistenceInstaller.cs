using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Ratings.Infrastructure;

public static class RatingsPersistenceInstaller
{
    /// <summary>Registers the module DbContext with the same provider options as its design-time factory.</summary>
    public static IServiceCollection AddRatingsPersistence(this IServiceCollection services, string? connectionString) =>
        services.AddDbContext<RatingsModuleDbContext>(options => options.UseModuleNpgsql(connectionString, RatingsModuleDbContext.SchemaName));
}
