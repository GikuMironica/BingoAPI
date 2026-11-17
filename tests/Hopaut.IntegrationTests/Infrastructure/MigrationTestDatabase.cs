using Mediator;
using Npgsql;

namespace Hopaut.IntegrationTests.Infrastructure;

/// <summary>Helpers for migration tests that run a module's migrations on their own database.</summary>
public static class MigrationTestDatabase
{
    /// <summary>The first two columns of every row the SQL returns, as text.</summary>
    public static async Task<List<(string?, string?)>> RowsAsync(string connectionString, string sql, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new List<(string?, string?)>();
        while (await reader.ReadAsync(ct))
        {
            rows.Add((reader.IsDBNull(0) ? null : reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1)));
        }

        return rows;
    }

    /// <summary>A publisher for module DbContexts built outside the host, which never dispatch domain events in these tests.</summary>
    public sealed class NoPublisher : IPublisher
    {
        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => ValueTask.CompletedTask;

        public ValueTask Publish(object notification, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }
}
