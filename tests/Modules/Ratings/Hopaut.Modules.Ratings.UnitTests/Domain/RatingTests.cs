using AwesomeAssertions.Execution;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.Modules.Ratings.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.UnitTests.Domain;

public sealed class RatingTests
{
    internal static readonly DateTimeOffset Now = new(2030, 1, 9, 12, 0, 0, TimeSpan.Zero);
    internal static readonly RatingId Id = RatingId.From(17);
    internal static readonly PostId Post = PostId.From(7);
    internal static readonly UserId Host = UserId.From("host-1");
    internal static readonly UserId Rater = UserId.From("guest-1");

    internal static RatedPost Attended(bool alreadyRated = false) => new(Post, Host, RaterAttended: true, alreadyRated);

    internal static RatingValue Rate(int value) => RatingValue.Create(value).Value;

    [Fact]
    public void Create_ByAnAcceptedAttendee_RatesTheHost()
    {
        // Arrange
        var feedback = RatingFeedback.Create("Great host").Value;

        // Act
        var rating = Rating.Create(Id, Rate(4), Attended(), Rater, feedback, Now).Value;

        // Assert
        using (new AssertionScope())
        {
            rating.Id.Should().Be(Id);
            rating.UserId.Should().Be(Host);
            rating.RaterId.Should().Be(Rater);
            rating.PostId.Should().Be(Post);
            rating.Rate.Value.Should().Be(4);
            rating.Feedback.Should().Be(feedback);
            rating.CreatedAt.Should().Be(Now);
            rating.DomainEvents.Should().ContainSingle().Which.Should().BeEquivalentTo(
                new RatingCreatedDomainEvent(Now, Id, Host, Rater, Post, Rate(4)), options => options.Excluding(e => e.EventId));
        }
    }

    [Fact]
    public void Create_ByTheHost_IsRefused()
    {
        // Act
        var result = Rating.Create(Id, Rate(5), Attended(), Host, feedback: null, Now);

        // Assert
        result.Error.Should().Be(RatingErrors.SelfRating(Post));
    }

    [Fact]
    public void Create_BySomeoneWhoDidNotAttend_IsForbidden()
    {
        // Act
        var result = Rating.Create(Id, Rate(5), Attended() with { RaterAttended = false }, Rater, feedback: null, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(RatingErrors.NotAttendee(Post));
            result.Error.Type.Should().Be(ErrorType.Forbidden);
        }
    }

    [Fact]
    public void Create_ASecondTime_IsAlreadyRated()
    {
        // Act
        var result = Rating.Create(Id, Rate(5), Attended(alreadyRated: true), Rater, feedback: null, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(RatingErrors.AlreadyRated(Post));
            result.Error.Type.Should().Be(ErrorType.Conflict);
        }
    }

    [Fact]
    public void Create_WithoutAnId_IsAProgrammingError()
    {
        // Act
        var act = () => Rating.Create(default, Rate(5), Attended(), Rater, feedback: null, Now);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Delete_RaisesDeletedWithTheRate()
    {
        // Arrange
        var rating = Rating.Create(Id, Rate(3), Attended(), Rater, feedback: null, Now).Value;
        rating.ClearDomainEvents();
        var later = Now.AddDays(1);

        // Act
        rating.Delete(later);

        // Assert
        rating.DomainEvents.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new RatingDeletedDomainEvent(later, Id, Host, Rate(3)), options => options.Excluding(e => e.EventId));
    }

    [Theory]
    [InlineData("  Great host  ", "Great host")]
    [InlineData("x", "x")]
    public void RatingFeedback_IsTrimmed(string input, string expected) =>
        RatingFeedback.Create(input).Value.Value.Should().Be(expected);

    [Fact]
    public void RatingFeedback_OfTheMaximumLength_IsAccepted() =>
        RatingFeedback.Create(new string('x', RatingFeedback.MaxLength)).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void RatingFeedback_WhenBlank_IsEmpty(string? input) =>
        RatingFeedback.Create(input).Error.Should().Be(RatingErrors.FeedbackEmpty);

    [Fact]
    public void RatingFeedback_LongerThanTheMaximum_IsTooLong() =>
        RatingFeedback.Create(new string('x', RatingFeedback.MaxLength + 1)).Error.Should().Be(RatingErrors.FeedbackTooLong);
}
