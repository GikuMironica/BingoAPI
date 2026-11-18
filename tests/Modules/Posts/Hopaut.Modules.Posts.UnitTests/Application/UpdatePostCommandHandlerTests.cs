using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Authorization;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Application.Commands.UpdatePost;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Domain.Events;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class UpdatePostCommandHandlerTests
{
    private static readonly DateTimeOffset Now = PostBuilder.Now;

    private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
    private readonly ITagRepository _tags = Substitute.For<ITagRepository>();
    private readonly IAttendanceQueries _attendance = Substitute.For<IAttendanceQueries>();
    private readonly IResourceAuthorizer _authorizer = Substitute.For<IResourceAuthorizer>();
    private readonly UpdatePostCommandHandler _handler;

    public UpdatePostCommandHandlerTests()
    {
        _tags.GetOrCreateAsync(Arg.Any<IReadOnlyCollection<TagName>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IReadOnlyCollection<TagName>>().Select((name, index) => Tag.Create(TagId.From(index + 10), name)).ToList());
        _handler = new UpdatePostCommandHandler(_posts, _tags, _attendance, _authorizer, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task Handle_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await _handler.Handle(Command(PostId.From(5)), TestContext.Current.CancellationToken);

        // Assert
        result.Error.Code.Should().Be("posts.post.not_found");
    }

    [Fact]
    public async Task Handle_WhenTheCallerMayNotOwnThePost_IsForbiddenAndChangesNothing()
    {
        // Arrange
        var post = Existing(authorized: false);

        // Act
        var result = await _handler.Handle(Command(post.Id, @event: Event(title: "Renamed")), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.not_host");
            post.Event.Title.Value.Should().Be("Garden party");
        }
    }

    [Fact]
    public async Task Handle_WithSeveralInvalidFields_ReportsThemTogetherAndCreatesNoTag()
    {
        // Arrange
        var post = Existing();
        var command = Command(
            post.Id,
            location: new CreatePostLocationInput(null, null, null, null, null, 13.4, 95),
            @event: Event(title: " ", slots: 0),
            tags: [new string('x', TagName.MaxLength + 1)]);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>()
                .Which.Errors.Keys.Should().BeEquivalentTo("location", "event.title", "event.slots", "tagNames");
            await _tags.DidNotReceive().GetOrCreateAsync(Arg.Any<IReadOnlyCollection<TagName>>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Handle_WithOnlyANewEnd_KeepsTheStart()
    {
        // Arrange
        var post = Existing();
        var start = post.Schedule.Start;

        // Act
        var result = await _handler.Handle(Command(post.Id, end: start.AddHours(6)), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.Schedule.Start.Should().Be(start);
            post.Schedule.End.Should().Be(start.AddHours(6));
        }
    }

    [Fact]
    public async Task Handle_WithFewerSlotsThanAcceptedAttendees_ReturnsTheRule()
    {
        // Arrange
        var post = Existing();
        _attendance.CountAcceptedAsync(post.Id, Arg.Any<CancellationToken>()).Returns(6);

        // Act
        var result = await _handler.Handle(Command(post.Id, @event: Event(slots: 5)), TestContext.Current.CancellationToken);

        // Assert
        result.Error.Code.Should().Be("posts.post.slots_below_attendees");
    }

    [Fact]
    public async Task Handle_WithoutNewSlots_DoesNotCountAttendees()
    {
        // Arrange
        var post = Existing();

        // Act
        await _handler.Handle(Command(post.Id, @event: Event(title: "Renamed")), TestContext.Current.CancellationToken);

        // Assert
        await _attendance.DidNotReceive().CountAcceptedAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithOnlyANewCurrency_KeepsTheAmount()
    {
        // Arrange
        var post = Existing(fee: Money.Create(12.5m, Currency.Create("EUR").Value).Value);

        // Act
        var result = await _handler.Handle(Command(post.Id, @event: Event(currency: 2)), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.Event.EntranceFee.Should().Be(Money.Create(12.5m, Currency.Create("CHF").Value).Value);
        }
    }

    [Fact]
    public async Task Handle_WithTagNames_RetagsThePostAndRaisesUpdated()
    {
        // Arrange
        var post = Existing();

        // Act
        var result = await _handler.Handle(Command(post.Id, tags: ["Jazz", "jazz"]), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.Tags.Select(link => link.TagId.Value).Should().Equal(10);
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().ContainSingle(e => !e.NotifyAttendees);
        }
    }

    private Post Existing(bool authorized = true, Money? fee = null)
    {
        var builder = new PostBuilder();
        var post = (fee is null ? builder : builder.Charging(fee)).Build();
        _posts.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _authorizer.IsAuthorizedAsync(post, PostPolicies.MustOwnPost, Arg.Any<CancellationToken>()).Returns(authorized);
        return post;
    }

    private static UpdatePostEventInput Event(string? title = null, int? slots = null, int? currency = null) =>
        new(title, Description: null, Requirements: null, slots, EntrancePrice: null, currency);

    private static UpdatePostCommand Command(
        PostId postId,
        DateTimeOffset? end = null,
        CreatePostLocationInput? location = null,
        UpdatePostEventInput? @event = null,
        IReadOnlyList<string>? tags = null) => new(postId, EventTime: null, end, location, @event, tags);
}
