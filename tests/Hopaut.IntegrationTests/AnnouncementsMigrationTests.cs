using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Announcements.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using static Hopaut.IntegrationTests.Infrastructure.MigrationTestDatabase;

namespace Hopaut.IntegrationTests;

/// <summary>
/// Plan Step 8.5 (D-034): the Announcements data conversion runs on stored rows in both directions, and every message it
/// keeps passes the <c>AnnouncementMessage</c> factory exactly (white space as .NET trims it, length in UTF-16 code units).
/// </summary>
[Collection(HostCollection.Name)]
public sealed class AnnouncementsMigrationTests(HopautApiFactory factory)
{
    private const string BeforeMessage = "20261117173756_AnnouncementIdSequence";
    private const string Emoji = "🎉";

    [Fact]
    public async Task GivenStoredMessages_WhenAnnouncementMessageEditedAtAndVersionRunsUpAndDown_ThenMessagesAreNormalisedAndReadable()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var database = new NpgsqlConnectionStringBuilder(factory.ConnectionString) { Database = "announcements_message_" + Guid.NewGuid().ToString("N") };
        var builder = new DbContextOptionsBuilder<AnnouncementsModuleDbContext>();
        builder.UseModuleNpgsql(database.ConnectionString, AnnouncementsModuleDbContext.SchemaName);
        await using var context = new AnnouncementsModuleDbContext(builder.Options, new NoPublisher());
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(BeforeMessage, ct);
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO announcements.announcements ("Id", "PostId", "Message", "CreatedAt") VALUES
                (1, 1, '  Doors open at eight  ', now()),
                (2, 1, '   ', now()),
                (3, 1, E'\t\n' || chr(160) || chr(12288), now()),
                (4, 1, chr(160) || 'Bring snacks' || E'\r\n', now()),
                (5, 1, repeat('x', 510), now()),
                (6, 1, repeat('🎉', 300), now()),
                (7, 1, 'x' || repeat('🎉', 300), now()),
                (8, 1, repeat('y', 499) || ' zz', now());
            """, ct);
        string[] expected =
        [
            "Doors open at eight",
            "Bring snacks",
            new string('x', 500),
            string.Concat(Enumerable.Repeat(Emoji, 250)),
            "x" + string.Concat(Enumerable.Repeat(Emoji, 249)),
            new string('y', 499),
        ];

        try
        {
            // Act
            await migrator.MigrateAsync(cancellationToken: ct);
            var stored = await RowsAsync(database.ConnectionString, """SELECT "Id"::text, "Message" FROM announcements.announcements ORDER BY "Id" """, ct);
            var read = (await context.Announcements.AsNoTracking().ToListAsync(ct))
                .OrderBy(a => a.Id.Value)
                .Select(a => (Id: a.Id.Value, Message: a.Message.Value, a.EditedAt))
                .ToList();
            var typeAfterUp = await ColumnTypeAsync(database.ConnectionString, ct);
            await migrator.MigrateAsync(BeforeMessage, ct);
            var typeAfterDown = await ColumnTypeAsync(database.ConnectionString, ct);

            // Assert
            using (new AssertionScope())
            {
                stored.Should().Equal(new[] { 1, 4, 5, 6, 7, 8 }.Zip(expected, (id, message) => ((string?)id.ToString(), (string?)message)));
                read.Select(a => a.Message).Should().Equal(expected);
                read.Select(a => a.Id).Should().Equal(1, 4, 5, 6, 7, 8);
                read.Should().OnlyContain(a => a.EditedAt == null);
                typeAfterUp.Should().Equal(("character varying", "500"));
                typeAfterDown.Should().Equal(("text", null));
            }
        }
        finally
        {
            await context.Database.EnsureDeletedAsync(ct);
        }
    }

    private static Task<List<(string?, string?)>> ColumnTypeAsync(string connectionString, CancellationToken ct) =>
        RowsAsync(
            connectionString,
            """
            SELECT data_type, character_maximum_length::text FROM information_schema.columns
            WHERE table_schema = 'announcements' AND table_name = 'announcements' AND column_name = 'Message'
            """,
            ct);
}
