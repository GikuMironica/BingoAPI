using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application;
using Hopaut.BuildingBlocks.Infrastructure.Outbox;
using Hopaut.IntegrationEvents;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Infrastructure;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Attendance.Infrastructure;
using Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;
using Hopaut.Modules.BugReports.Infrastructure;
using Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;
using Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;
using Hopaut.Modules.Moderation.Infrastructure;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Infrastructure;
using Hopaut.Modules.Ratings.Application.Commands.CreateRating;
using Hopaut.Modules.Ratings.Application.Commands.DeleteRating;
using Hopaut.Modules.Ratings.Infrastructure;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>
/// One transaction per command (plan Step 7.6): the module unit of work saves once after the handler, domain events are
/// dispatched before that save, and aggregates carry IDs from their module sequence before any event is raised.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class UnitOfWorkTests(HopautApiFactory factory)
{
    [Fact]
    public async Task GivenACreatePostCommand_WhenSent_ThenThePostAndItsCreatedEventAreSavedWithTheSequenceId()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var command = NewPost("uow-created-" + Guid.NewGuid());

        // Act
        var result = await SendAsync(factory.Services, command, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await using var scope = factory.Services.CreateAsyncScope();
        var saved = await scope.ServiceProvider.GetRequiredService<PostsModuleDbContext>().Posts
            .AnyAsync(p => p.Id == result.Value, ct);
        var createdEvents = await IntegrationEventsAsync<PostCreatedIntegrationEvent>(scope.ServiceProvider, ct);
        using (new AssertionScope())
        {
            result.Value.Value.Should().BePositive();
            saved.Should().BeTrue();
            createdEvents.Select(e => e.PostId).Should().Contain(result.Value, "the domain-event handler ran before the save with the pre-assigned ID");
        }
    }

    [Fact]
    public async Task GivenAFailingDomainEventHandler_WhenACommandIsSent_ThenNoRowIsCommitted()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var marker = "uow-rolled-back-" + Guid.NewGuid();
        await using var failingHost = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddScoped<IIntegrationEventPublisher, FailingIntegrationEventPublisher>()));

        // Act
        var act = () => SendAsync(failingHost.Services, NewPost(marker), ct);

        // Assert
        var thrown = await act.Should().ThrowAsync<Exception>();
        await using var scope = factory.Services.CreateAsyncScope();
        var description = PostDescription.Create(marker).Value;
        var committed = await scope.ServiceProvider.GetRequiredService<PostsModuleDbContext>().Events
            .AnyAsync(e => e.Description == description, ct);
        using (new AssertionScope())
        {
            SelfAndInner(thrown.Which).Should().ContainSingle(e => e is SimulatedPublishFailure);
            committed.Should().BeFalse("the handler failed before the unit of work saved");
        }
    }

    [Fact]
    public async Task GivenRatingsCreatedAndDeleted_WhenSent_ThenTheReputationIsSavedWithEachChange()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var suffix = Guid.NewGuid().ToString("N");
        var rated = UserId.From("uow-rated-" + suffix);
        var post = PostId.From(7);

        // Act
        var first = await SendAsync(factory.Services, new CreateRatingCommand(4, rated, UserId.From("rater-a-" + suffix), post, null), ct);
        await SendAsync(factory.Services, new CreateRatingCommand(2, rated, UserId.From("rater-b-" + suffix), post, null), ct);
        var afterCreates = await ReputationAsync(rated, ct);
        var deleted = await TestCaller.SendAsync(factory.Services, TestCaller.For("rater-a-" + suffix), new DeleteRatingCommand(first.Value), ct);
        var afterDelete = await ReputationAsync(rated, ct);

        // Assert
        using (new AssertionScope())
        {
            deleted.IsSuccess.Should().BeTrue();
            afterCreates.Should().Be((2, 6, 3.0));
            afterDelete.Should().Be((1, 2, 2.0));
        }
    }

    [Fact]
    public async Task GivenTheCreateCommandsOfEveryModule_WhenSent_ThenEachRowIsSavedUnderItsSequenceId()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var suffix = Guid.NewGuid().ToString("N");
        var reporter = UserId.From("reporter-" + suffix);
        var attendee = UserId.From("attendee-" + suffix);
        var post = PostId.From(11);
        var hostedPost = await SendAsync(factory.Services, NewPost("uow-announced-" + suffix), ct);

        // Act
        var announcement = await TestCaller.SendAsync(
            factory.Services, TestCaller.For(UnitOfWorkHost), new CreateAnnouncementCommand(hostedPost.Value, "uow"), ct);
        var bug = await SendAsync(factory.Services, new CreateBugReportCommand("uow", reporter, ["https://cdn.hopaut.app/bug.png"]), ct);
        var postReport = await SendAsync(factory.Services, new CreatePostReportCommand(post, reporter, UserId.From("host"), 1, null), ct);
        var userReport = await SendAsync(factory.Services, new CreateUserReportCommand(reporter, UserId.From("reported"), 2, null), ct);
        var attendance = await TestCaller.SendAsync(
            factory.Services, TestCaller.For(attendee.Value), new RequestAttendanceCommand(hostedPost.Value), ct);

        // Assert
        await using var scope = factory.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var participation = await services.GetRequiredService<AttendanceModuleDbContext>().Participations
            .SingleAsync(p => p.PostId == hostedPost.Value && p.UserId == attendee, ct);
        var requestedEvents = await IntegrationEventsAsync<AttendanceRequestedIntegrationEvent>(services, ct);
        using (new AssertionScope())
        {
            (await services.GetRequiredService<AnnouncementsModuleDbContext>().Announcements.AnyAsync(a => a.Id == announcement.Value, ct)).Should().BeTrue();
            (await services.GetRequiredService<BugReportsModuleDbContext>().Bugs.AnyAsync(b => b.Id == bug.Value, ct)).Should().BeTrue();
            (await services.GetRequiredService<ModerationModuleDbContext>().PostReports.AnyAsync(r => r.Id == postReport.Value, ct)).Should().BeTrue();
            (await services.GetRequiredService<ModerationModuleDbContext>().UserReports.AnyAsync(r => r.Id == userReport.Value, ct)).Should().BeTrue();
            attendance.IsSuccess.Should().BeTrue();
            participation.Id.Value.Should().BePositive();
            requestedEvents.Select(e => e.ParticipationId).Should().Contain(participation.Id, "the requested event carries the pre-assigned ID (F-007)");
        }
    }

    private const string UnitOfWorkHost = "uow-host";

    private static CreatePostCommand NewPost(string description) => new(
        new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2030, 1, 9, 23, 0, 0, TimeSpan.Zero),
        UserId.From(UnitOfWorkHost),
        new CreatePostLocationInput("Venue", "Berlin", null, "Main St 1", "DE", 13.405, 52.52),
        new CreatePostEventInput(EventType.HouseParty, null, null, "Unit of work", description, null, 10, null));

    private static async Task<TResponse> SendAsync<TResponse>(IServiceProvider root, ICommand<TResponse> command, CancellationToken ct)
    {
        await using var scope = root.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, ct);
    }

    private async Task<(int Total, int Sum, double Average)> ReputationAsync(UserId userId, CancellationToken ct)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var reputation = await scope.ServiceProvider.GetRequiredService<RatingsModuleDbContext>().UserReputations
            .SingleAsync(r => r.UserId == userId, ct);
        return (reputation.TotalRatings, reputation.SumRatings, reputation.AverageRating);
    }

    private static async Task<IReadOnlyList<TEvent>> IntegrationEventsAsync<TEvent>(IServiceProvider services, CancellationToken ct)
        where TEvent : IIntegrationEvent
    {
        var payloads = await services.GetRequiredService<OutboxDbContext>().OutboxMessages
            .Where(m => m.Type.StartsWith(typeof(TEvent).FullName!))
            .Select(m => m.Payload)
            .ToListAsync(ct);
        return payloads.Select(payload => JsonSerializer.Deserialize<TEvent>(payload)!).ToList();
    }

    private static IEnumerable<Exception> SelfAndInner(Exception exception) =>
        exception is AggregateException aggregate
            ? [aggregate, .. aggregate.InnerExceptions.SelectMany(SelfAndInner)]
            : exception.InnerException is null ? [exception] : [exception, .. SelfAndInner(exception.InnerException)];

    private sealed class SimulatedPublishFailure() : Exception("Simulated failure of a domain-event handler.");

    private sealed class FailingIntegrationEventPublisher : IIntegrationEventPublisher
    {
        public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) =>
            throw new SimulatedPublishFailure();
    }
}
