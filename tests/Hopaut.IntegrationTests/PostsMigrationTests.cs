using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Posts.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using static Hopaut.IntegrationTests.Infrastructure.MigrationTestDatabase;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 8.2: the Posts data conversions run on legacy-shaped rows in both directions.</summary>
[Collection(HostCollection.Name)]
public sealed class PostsMigrationTests(HopautApiFactory factory)
{
    private const string BeforeMoney = "20261117195201_PostStatusAndRequiredTitle";
    private const string BeforeTags = "20261117204341_PostVersion";

    [Fact]
    public async Task GivenLegacyTags_WhenTagNamesAndIdSequenceRunsUpAndDown_ThenNamesAreNormalisedDuplicatesMergedAndIdsContinue()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var database = new NpgsqlConnectionStringBuilder(factory.ConnectionString) { Database = "posts_tags_" + Guid.NewGuid().ToString("N") };
        var builder = new DbContextOptionsBuilder<PostsModuleDbContext>();
        builder.UseModuleNpgsql(database.ConnectionString, PostsModuleDbContext.SchemaName, PostsModuleDbContext.ConfigureNpgsql);
        await using var context = new PostsModuleDbContext(builder.Options, new NoPublisher());
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(BeforeTags, ct);
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO posts.posts ("Id", "PostTime", "EventTime", "EndTime", "Status", "UserId")
            SELECT id, now(), now(), now() + interval '2 hours', 1, 'host' FROM generate_series(1, 2) AS id;
            INSERT INTO posts.tags ("Id", "TagName") VALUES
                (1, ' Jazz '), (2, 'jazz'), (3, '   '), (4, 'ROCK'), (5, 'abcdefghijklmnopqrstuvwxyz');
            INSERT INTO posts.post_tags ("PostId", "TagId") VALUES (1, 1), (1, 2), (2, 2), (2, 3), (2, 4), (2, 5);
            """, ct);

        try
        {
            // Act
            await migrator.MigrateAsync(cancellationToken: ct);
            var tags = await RowsAsync(database.ConnectionString, """SELECT "Id"::text, "TagName" FROM posts.tags ORDER BY "Id" """, ct);
            var links = await RowsAsync(database.ConnectionString, """SELECT "PostId"::text, "TagId"::text FROM posts.post_tags ORDER BY 1, 2""", ct);
            var nextId = await RowsAsync(database.ConnectionString, "SELECT nextval('posts.tag_ids')::text, NULL", ct);
            await migrator.MigrateAsync(BeforeTags, ct);
            var restoredIdentity = await RowsAsync(database.ConnectionString, """INSERT INTO posts.tags ("TagName") VALUES ('after down') RETURNING "Id"::text, "TagName" """, ct);

            // Assert
            using (new AssertionScope())
            {
                tags.Should().Equal(("1", "jazz"), ("4", "rock"), ("5", "abcdefghijklmnopqrst"));
                links.Should().Equal(("1", "1"), ("2", "1"), ("2", "4"), ("2", "5"));
                nextId.Should().Equal(("6", null));
                restoredIdentity.Should().Equal(("6", "after down"));
            }
        }
        finally
        {
            await context.Database.EnsureDeletedAsync(ct);
        }
    }

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
}
