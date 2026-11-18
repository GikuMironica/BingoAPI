using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Ratings.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using static Hopaut.IntegrationTests.Infrastructure.MigrationTestDatabase;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 8.4: the Ratings data conversion runs on legacy-shaped rows in both directions.</summary>
[Collection(HostCollection.Name)]
public sealed class RatingsMigrationTests(HopautApiFactory factory)
{
    private const string BeforeFeedback = "20261117173752_RatingIdSequence";

    [Fact]
    public async Task GivenLegacyFeedback_WhenRatingFeedbackAndReputationVersionRunsUpAndDown_ThenFeedbackIsNormalisedAndReadable()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var database = new NpgsqlConnectionStringBuilder(factory.ConnectionString) { Database = "ratings_feedback_" + Guid.NewGuid().ToString("N") };
        var builder = new DbContextOptionsBuilder<RatingsModuleDbContext>();
        builder.UseModuleNpgsql(database.ConnectionString, RatingsModuleDbContext.SchemaName);
        await using var context = new RatingsModuleDbContext(builder.Options, new NoPublisher());
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(BeforeFeedback, ct);
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO ratings.ratings ("Id", "Rate", "UserId", "RaterId", "PostId", "Feedback", "CreatedAt") VALUES
                (1, 5, 'host', 'rater-a', 1, '  Great host  ', now()),
                (2, 4, 'host', 'rater-b', 1, '   ', now()),
                (3, 3, 'host', 'rater-c', 1, NULL, now()),
                (4, 2, 'host', 'rater-d', 1, repeat('x', 310), now());
            """, ct);

        try
        {
            // Act
            await migrator.MigrateAsync(cancellationToken: ct);
            var stored = await RowsAsync(database.ConnectionString, """SELECT "Id"::text, "Feedback" FROM ratings.ratings ORDER BY "Id" """, ct);
            var read = await context.Ratings.AsNoTracking().OrderBy(r => r.CreatedAt).ThenBy(r => r.RaterId).Select(r => r.Feedback).ToListAsync(ct);
            await migrator.MigrateAsync(BeforeFeedback, ct);
            var columnType = await RowsAsync(
                database.ConnectionString,
                "SELECT data_type, NULL FROM information_schema.columns WHERE table_schema = 'ratings' AND table_name = 'ratings' AND column_name = 'Feedback'",
                ct);

            // Assert
            using (new AssertionScope())
            {
                stored.Should().Equal(("1", "Great host"), ("2", null), ("3", null), ("4", new string('x', 300)));
                read.Select(feedback => feedback?.Value).Should().BeEquivalentTo(["Great host", null, null, new string('x', 300)]);
                columnType.Should().Equal(("text", null));
            }
        }
        finally
        {
            await context.Database.EnsureDeletedAsync(ct);
        }
    }
}
