using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Hopaut.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` create a module DbContext without booting the Host. Generating migrations needs no database;
/// commands that do (e.g. `database update`) read the connection string from <see cref="ConnectionVariable"/>.
/// </summary>
public abstract class ModuleDesignTimeDbContextFactory<TContext>(string schema, Action<NpgsqlDbContextOptionsBuilder>? configureNpgsql = null)
    : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public const string ConnectionVariable = "HOPAUT_DESIGN_CONNECTION";

    private const string PlaceholderConnection = "Host=localhost;Database=hopaut_design";

    public TContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<TContext>();
        builder.UseModuleNpgsql(Environment.GetEnvironmentVariable(ConnectionVariable) ?? PlaceholderConnection, schema, configureNpgsql);
        return Create(builder.Options);
    }

    protected abstract TContext Create(DbContextOptions<TContext> options);

    /// <summary>Domain events are never dispatched at design time.</summary>
    protected sealed class DesignTimePublisher : IPublisher
    {
        public static readonly DesignTimePublisher Instance = new();

        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => ValueTask.CompletedTask;

        public ValueTask Publish(object notification, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }
}
