using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Application.Authorization;
using Hopaut.Modules.Ratings.Application.Commands.CreateRating;
using Hopaut.Modules.Ratings.Application.Commands.DeleteRating;
using Hopaut.Modules.Ratings.Application.Queries;
using Hopaut.Modules.Ratings.Application.Queries.GetRatingById;
using Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.Modules.Ratings.Domain.Events;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using static Hopaut.Modules.Ratings.UnitTests.Domain.RatingTests;

namespace Hopaut.Modules.Ratings.UnitTests.Application;

public sealed class RatingHandlerTests
{
    private readonly IRatingRepository _ratings = Substitute.For<IRatingRepository>();
    private readonly IPostHostQueries _posts = Substitute.For<IPostHostQueries>();
    private readonly IAttendanceQueries _attendance = Substitute.For<IAttendanceQueries>();
    private readonly IResourceAuthorizer _authorizer = Substitute.For<IResourceAuthorizer>();
    private readonly IRatingReadModel _readModel = Substitute.For<IRatingReadModel>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly FakeTimeProvider _time = new(Now);

    public RatingHandlerTests()
    {
        _currentUser.Id.Returns(Rater);
        _ratings.NextIdAsync(Arg.Any<CancellationToken>()).Returns(Id);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Create_WithAnInvalidRateAndFeedback_ReportsBothFieldsAndLooksNothingUp()
    {
        // Act
        var result = await CreateHandler().Handle(new CreateRatingCommand(6, Post, new string('x', 301)), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("rate", "feedback");
            await _posts.DidNotReceive().GetAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Create_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await CreateHandler().Handle(new CreateRatingCommand(4, Post, null), Ct);

        // Assert
        result.Error.Should().Be(RatingErrors.PostNotFound(Post));
    }

    [Fact]
    public async Task Create_ByACallerWhoDidNotAttend_IsForbiddenAndAddsNothing()
    {
        // Arrange
        PostHostedBy(Host, callerAttended: false);

        // Act
        var result = await CreateHandler().Handle(new CreateRatingCommand(4, Post, null), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(RatingErrors.NotAttendee(Post));
            _ratings.DidNotReceive().Add(Arg.Any<Rating>());
        }
    }

    [Fact]
    public async Task Create_ASecondTime_IsAlreadyRated()
    {
        // Arrange
        PostHostedBy(Host, callerAttended: true);
        _ratings.ExistsAsync(Rater, Post, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await CreateHandler().Handle(new CreateRatingCommand(4, Post, null), Ct);

        // Assert
        result.Error.Should().Be(RatingErrors.AlreadyRated(Post));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData(" Great host ", "Great host")]
    public async Task Create_ByAnAttendee_AddsTheCallersRatingOfTheHost(string? feedback, string? expected)
    {
        // Arrange
        PostHostedBy(Host, callerAttended: true);

        // Act
        var result = await CreateHandler().Handle(new CreateRatingCommand(4, Post, feedback), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Should().Be(Id);
            _ratings.Received(1).Add(Arg.Is<Rating>(r =>
                r.Id == Id && r.UserId == Host && r.RaterId == Rater && r.Rate.Value == 4 && (r.Feedback == null ? null : r.Feedback.Value) == expected));
        }
    }

    [Fact]
    public async Task Delete_AMissingRating_IsNotFound()
    {
        // Act
        var result = await DeleteHandler().Handle(new DeleteRatingCommand(Id), Ct);

        // Assert
        result.Error.Should().Be(RatingErrors.NotFound(Id));
    }

    [Fact]
    public async Task Delete_BySomeoneOtherThanTheRaterOrAnAdministrator_IsForbiddenAndKeepsIt()
    {
        // Arrange
        var rating = Existing(authorized: false);

        // Act
        var result = await DeleteHandler().Handle(new DeleteRatingCommand(Id), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(RatingErrors.NotRater(Id));
            _ratings.DidNotReceive().Remove(Arg.Any<Rating>());
            rating.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Delete_ByTheRater_RemovesItAndRaisesDeleted()
    {
        // Arrange
        var rating = Existing(authorized: true);

        // Act
        var result = await DeleteHandler().Handle(new DeleteRatingCommand(Id), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            _ratings.Received(1).Remove(rating);
            rating.DomainEvents.OfType<RatingDeletedDomainEvent>().Should().ContainSingle();
        }
    }

    [Fact]
    public async Task GetById_ForAMissingRating_IsNotFound()
    {
        // Act
        var result = await new GetRatingByIdQueryHandler(_readModel).Handle(new GetRatingByIdQuery(Id), Ct);

        // Assert
        result.Error.Should().Be(RatingErrors.NotFound(Id));
    }

    [Fact]
    public async Task GetById_ReturnsTheRating()
    {
        // Arrange
        var dto = new RatingDto(Id.Value, 4, Rater.Value, Post.Value, null, Now);
        _readModel.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(dto);

        // Act
        var result = await new GetRatingByIdQueryHandler(_readModel).Handle(new GetRatingByIdQuery(Id), Ct);

        // Assert
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetByUser_ReturnsTheRatingsTheUserReceived()
    {
        // Arrange
        IReadOnlyList<RatingDto> received = [new RatingDto(Id.Value, 4, Rater.Value, Post.Value, "Great host", Now)];
        _readModel.GetReceivedAsync(Host, Arg.Any<CancellationToken>()).Returns(received);

        // Act
        var result = await new GetRatingsByUserQueryHandler(_readModel).Handle(new GetRatingsByUserQuery(Host), Ct);

        // Assert
        result.Value.Should().Equal(received);
    }

    private CreateRatingCommandHandler CreateHandler() => new(_ratings, _posts, _attendance, _currentUser, _time);

    private DeleteRatingCommandHandler DeleteHandler() => new(_ratings, _authorizer, _time);

    private void PostHostedBy(UserId host, bool callerAttended)
    {
        _posts.GetAsync(Post, Arg.Any<CancellationToken>()).Returns(new PostHost(Post, host));
        _attendance.IsAcceptedAsync(Post, Rater, Arg.Any<CancellationToken>()).Returns(callerAttended);
    }

    private Rating Existing(bool authorized)
    {
        var rating = Rating.Create(Id, Rate(4), Attended(), Rater, feedback: null, Now).Value;
        rating.ClearDomainEvents();
        _ratings.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(rating);
        _authorizer.IsAuthorizedAsync(rating, RatingPolicies.MustOwnRating, Arg.Any<CancellationToken>()).Returns(authorized);
        return rating;
    }
}
