using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hopaut.BuildingBlocks.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    /// <summary>Every DbContext registered with AddDbContext (EF registers <c>DbContextOptions&lt;TContext&gt;</c> for each).</summary>
    public static IReadOnlyList<Type> RegisteredDbContexts(IServiceCollection services) =>
        services
            .Select(d => d.ServiceType)
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
            .Select(t => t.GetGenericArguments()[0])
            .Distinct()
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Applies pending migrations of every given context. Meant for Development and tests only
    /// (configuration switch <c>Database:MigrateOnStartup</c>); production applies reviewed scripts.
    /// </summary>
    public static async Task MigrateAsync(
        IServiceProvider services, IEnumerable<Type> dbContextTypes, ILogger logger, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        foreach (var contextType in dbContextTypes)
        {
            var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations applied for {DbContext}", contextType.Name);
        }
    }
}
