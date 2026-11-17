using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure.Outbox;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Announcements.Infrastructure;
using Hopaut.Modules.Attendance.Infrastructure;
using Hopaut.Modules.BugReports.Infrastructure;
using Hopaut.Modules.Identity.Infrastructure;
using Hopaut.Modules.Moderation.Infrastructure;
using Hopaut.Modules.Posts.Infrastructure;
using Hopaut.Modules.Ratings.Infrastructure;
using Hopaut.Modules.Users.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

[Collection(HostCollection.Name)]
public sealed class DatabaseMigrationTests(HopautApiFactory factory)
{
    private static readonly Type[] MigratedContexts =
    [
        typeof(IdentityModuleDbContext), typeof(UsersModuleDbContext), typeof(PostsModuleDbContext),
        typeof(AttendanceModuleDbContext), typeof(AnnouncementsModuleDbContext), typeof(RatingsModuleDbContext),
        typeof(ModerationModuleDbContext), typeof(BugReportsModuleDbContext), typeof(OutboxDbContext),
    ];

    public static TheoryData<string> ContextNames() => new(MigratedContexts.Select(t => t.Name));

    [Theory]
    [MemberData(nameof(ContextNames))]
    public async Task GivenMigrateOnStartup_WhenTheHostHasStarted_ThenTheContextHasNoPendingMigrations(string contextName)
    {
        // Arrange
        var contextType = MigratedContexts.Single(t => t.Name == contextName);
        await using var scope = factory.Services.CreateAsyncScope();
        var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);

        // Act
        var pending = await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);
        var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            pending.Should().BeEmpty();
            applied.Should().ContainSingle(m => m.EndsWith("_InitialCreate", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void GivenTheSolution_WhenItsDbContextsAreListed_ThenEveryOneIsMigratedByThisSuite()
    {
        // Arrange
        _ = factory.Services; // the Host is started, so every module assembly is loaded

        // Act
        var contexts = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("Hopaut.", StringComparison.Ordinal) == true)
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false } && typeof(DbContext).IsAssignableFrom(t))
            .ToList();

        // Assert
        contexts.Should().BeEquivalentTo(MigratedContexts);
    }

    [Fact]
    public async Task GivenMigratedIdentitySchema_WhenTheHostHasStarted_ThenTheDefaultRolesAreSeeded()
    {
        // Arrange
        await using var scope = factory.Services.CreateAsyncScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Act
        var names = await roles.Roles.Select(r => r.Name).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        names.Should().BeEquivalentTo(["Admin", "User", "SuperAdmin"]);
    }
}
