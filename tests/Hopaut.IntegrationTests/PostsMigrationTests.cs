using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Posts.Infrastructure;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 8.2: the Posts data conversions run on legacy-shaped rows in both directions.</summary>
[Collection(HostCollection.Name)]
public sealed class PostsMigrationTests(HopautApiFactory factory)
{
    private const string BeforeMoney = "20261117195201_PostStatusAndRequiredTitle";

    [Fact]
    public async Task GivenLegacyEntrancePrices_WhenEntranceFeeMoneyRunsUpAndDown_ThenFeesConvertBothWays()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var database = new NpgsqlConnectionStringBuilder(factory.ConnectionString) { Database = "posts_migration_" + Guid.NewGuid().ToString("N") };
        var builder = new DbContextOptionsBuilder<PostsModuleDbContext>();
        builder.UseModuleNpgsql(database.ConnectionString, PostsModuleDbContext.SchemaName, PostsModuleDbContext.ConfigureNpgsql);
        await using var context = new PostsModuleDbContext(builder.Options, new NoPublisher());
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(BeforeMoney, ct);
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO posts.posts ("Id", "PostTime", "EventTime", "EndTime", "Status", "UserId")
            SELECT id, now(), now(), now() + interval '2 hours', 1, 'host' FROM generate_series(1, 5) AS id;
            INSERT INTO posts.events ("EventType", "EntrancePrice", "Currency", "Title", "Description", "PostId") VALUES
                (1, 12.345, 2, 'Franc party', 'Ten or more characters', 1),
                (1, 5, 8, 'Lev party', 'Ten or more characters', 2),
                (1, 7.5, NULL, 'No currency', 'Ten or more characters', 3),
                (1, -3, 0, 'Negative', 'Ten or more characters', 4),
                (1, NULL, 3, 'Free', 'Ten or more characters', 5);
            """, ct);

        try
        {
            // Act
            await migrator.MigrateAsync(cancellationToken: ct);
            var up = await RowsAsync(database.ConnectionString, """SELECT entrance_fee_amount::text, entrance_fee_currency::text FROM posts.events ORDER BY "PostId" """, ct);
            await migrator.MigrateAsync(BeforeMoney, ct);
            var down = await RowsAsync(database.ConnectionString, """SELECT "EntrancePrice"::text, "Currency"::text FROM posts.events ORDER BY "PostId" """, ct);

            // Assert
            using (new AssertionScope())
            {
                up.Should().Equal(("12.35", "CHF"), ("5.00", "BGN"), ("7.50", "EUR"), ("0.00", "EUR"), (null, null));
                down.Should().Equal(("12.35", "2"), ("5", "8"), ("7.5", "0"), ("0", "0"), (null, null));
            }
        }
        finally
        {
            await context.Database.EnsureDeletedAsync(ct);
        }
    }

    private static async Task<List<(string?, string?)>> RowsAsync(string connectionString, string sql, CancellationToken ct)
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

    private sealed class NoPublisher : IPublisher
    {
        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => ValueTask.CompletedTask;

        public ValueTask Publish(object notification, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }
}
