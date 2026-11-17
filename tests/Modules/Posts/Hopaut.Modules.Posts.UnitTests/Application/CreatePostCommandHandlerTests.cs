using AwesomeAssertions.Execution;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class CreatePostCommandHandlerTests
{
    private static readonly DateTimeOffset Now = PostBuilder.Now;

    private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
    private readonly CreatePostCommandHandler _handler;

    public CreatePostCommandHandlerTests()
    {
        _posts.NextIdAsync(Arg.Any<CancellationToken>()).Returns(PostId.From(11));
        _handler = new CreatePostCommandHandler(_posts, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task Handle_WithValidInput_AddsThePostUnderTheSequenceId()
    {
        // Act
        var result = await _handler.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Should().Be(PostId.From(11));
            _posts.Received(1).Add(Arg.Is<Post>(p => p.Id == PostId.From(11) && p.UserId == UserId.From("host-1")));
        }
    }

    [Fact]
    public async Task Handle_WithSeveralInvalidFields_ReportsThemTogetherAndSavesNothing()
    {
        // Arrange
        var command = Command(title: null, description: "too short", slots: 0, latitude: 91);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>()
                .Which.Errors.Keys.Should().BeEquivalentTo("location", "event.title", "event.description", "event.slots");
            await _posts.DidNotReceive().NextIdAsync(Arg.Any<CancellationToken>());
            _posts.DidNotReceive().Add(Arg.Any<Post>());
        }
    }

    [Fact]
    public async Task Handle_BreakingAScheduleRule_ReturnsTheRuleAndSavesNothing()
    {
        // Arrange
        var command = Command(end: Now.AddDays(1).AddMinutes(10));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.too_short");
            _posts.DidNotReceive().Add(Arg.Any<Post>());
        }
    }

    [Fact]
    public async Task Handle_WithAnEndBeforeTheStart_ReportsTheEndTimeField()
    {
        // Act
        var result = await _handler.Handle(Command(end: Now), TestContext.Current.CancellationToken);

        // Assert
        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().Equal("endTime");
    }

    private static CreatePostCommand Command(
        string? title = "Garden party",
        string description = "Bring snacks and good vibes",
        int? slots = 20,
        double latitude = 52.52,
        DateTimeOffset? end = null) => new(
        Now.AddDays(1),
        end ?? Now.AddDays(1).AddHours(4),
        UserId.From("host-1"),
        new CreatePostLocationInput("Venue", "Berlin", null, "Main St 1", "DE", 13.405, latitude),
        new CreatePostEventInput(EventType.HouseParty, null, null, title, description, null, slots, null));
}
