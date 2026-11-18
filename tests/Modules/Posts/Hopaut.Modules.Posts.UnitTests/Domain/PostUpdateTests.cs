using AwesomeAssertions.Execution;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.UnitTests.Domain;

/// <summary>The host's edit with the legacy update rules (pre-net10-baseline: UpdatedPostDetailsWatcher).</summary>
public sealed class PostUpdateTests
{
    private static readonly DateTimeOffset Now = PostBuilder.Now;

    [Fact]
    public void Update_BeforeTheStart_MovesTheScheduleAndAsksToNotifyAttendees()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var schedule = Range(Now.AddDays(2), Now.AddDays(2).AddHours(3));

        // Act
        var result = post.Update(new PostRevision { Schedule = schedule }, acceptedGuests: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.Schedule.Should().Be(schedule);
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().ContainSingle(e => e.NotifyAttendees && e.PostId == post.Id);
        }
    }

    [Theory]
    [InlineData(5, null)]
    [InlineData(6, "posts.post.moved_to_the_past")]
    public void Update_BeforeTheStart_AllowsAStartAtMostFiveMinutesInThePast(int minutesAgo, string? code)
    {
        // Arrange
        var post = new PostBuilder().Build();
        var start = Now.AddMinutes(-minutesAgo);

        // Act
        var result = post.Update(new PostRevision { Schedule = Range(start, start.AddHours(1)) }, acceptedGuests: 0, Now);

        // Assert
        (result.IsSuccess ? null : result.Error.Code).Should().Be(code);
    }

    [Fact]
    public void Update_AfterTheStart_RefusesANewStart()
    {
        // Arrange
        var post = Started();

        // Act
        var result = post.Update(new PostRevision { Schedule = Range(Now.AddMinutes(-5), Now.AddHours(2)) }, acceptedGuests: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.already_started");
            result.Error.Type.Should().Be(ErrorType.Rule);
        }
    }

    [Theory]
    [InlineData(29, true)]
    [InlineData(28, false)]
    public void Update_AfterTheStart_MovesTheEndToAtLeastAboutThirtyMinutesFromNow(int minutesFromNow, bool allowed)
    {
        // Arrange
        var post = Started();

        // Act
        var result = post.Update(new PostRevision { Schedule = Range(post.Schedule.Start, Now.AddMinutes(minutesFromNow)) }, acceptedGuests: 0, Now);

        // Assert
        (result.IsSuccess ? null : result.Error.Code).Should().Be(allowed ? null : "posts.post.ends_too_soon");
    }

    [Fact]
    public void Update_BreakingTheDurationRule_KeepsTheOldSchedule()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var original = post.Schedule;

        // Act
        var result = post.Update(new PostRevision { Schedule = Range(Now.AddDays(2), Now.AddDays(2).AddHours(12)) }, acceptedGuests: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.too_long");
            post.Schedule.Should().Be(original);
        }
    }

    [Fact]
    public void Update_WithFewerSlotsThanAcceptedAttendees_IsRefusedAndChangesNothing()
    {
        // Arrange
        var post = new PostBuilder().Build();

        // Act
        var result = post.Update(
            new PostRevision { Slots = Capacity.Create(3).Value, Title = PostTitle.Create("Renamed").Value },
            acceptedGuests: 4,
            Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.slots_below_attendees");
            post.Event.Slots?.Value.Should().Be(20);
            post.Event.Title.Value.Should().Be("Garden party");
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().BeEmpty();
        }
    }

    [Fact]
    public void Update_WithSlotsEqualToAcceptedAttendees_Succeeds()
    {
        // Arrange
        var post = new PostBuilder().Build();

        // Act
        var result = post.Update(new PostRevision { Slots = Capacity.Create(4).Value }, acceptedGuests: 4, Now);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.Event.Slots?.Value.Should().Be(4);
        }
    }

    [Fact]
    public void Update_ChangingOnlyTheTitle_RaisesUpdatedWithoutNotifyingAttendees()
    {
        // Arrange
        var post = new PostBuilder().Build();

        // Act
        post.Update(new PostRevision { Title = PostTitle.Create("Renamed").Value }, acceptedGuests: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            post.Event.Title.Value.Should().Be("Renamed");
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().ContainSingle(e => !e.NotifyAttendees);
        }
    }

    [Fact]
    public void Update_ChangingTheRequirements_AsksToNotifyAttendees()
    {
        // Arrange
        var post = new PostBuilder().Build();

        // Act
        post.Update(new PostRevision { Requirements = "Bring a tent" }, acceptedGuests: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            post.Event.Requirements.Should().Be("Bring a tent");
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().ContainSingle(e => e.NotifyAttendees);
        }
    }

    [Fact]
    public void Update_ChangingTheFee_AsksToNotifyAttendees()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var fee = Money.Create(12.5m, Currency.Create("EUR").Value).Value;

        // Act
        post.Update(new PostRevision { EntranceFee = fee }, acceptedGuests: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            post.Event.EntranceFee.Should().Be(fee);
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().ContainSingle(e => e.NotifyAttendees);
        }
    }

    [Fact]
    public void Update_MovingTheLocation_ChangesThePointAndAsksToNotifyAttendees()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var location = new PostLocation(Coordinates.Create(48.137, 11.575).Value, new PostPlace("Hall", "Munich", null, "Square 2", "DE"));

        // Act
        post.Update(new PostRevision { Location = location }, acceptedGuests: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            post.Location.Location.Y.Should().Be(48.137);
            post.Location.City.Should().Be("Munich");
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().ContainSingle(e => e.NotifyAttendees);
        }
    }

    [Fact]
    public void Update_WithValuesItAlreadyHas_RaisesNothing()
    {
        // Arrange
        var post = new PostBuilder().WithTags(1).Build();

        // Act
        var result = post.Update(
            new PostRevision { Title = PostTitle.Create("Garden party").Value, Schedule = post.Schedule, Tags = [TagId.From(1)] },
            acceptedGuests: 0,
            Now);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.DomainEvents.OfType<PostUpdatedDomainEvent>().Should().BeEmpty();
        }
    }

    [Fact]
    public void Update_WithRequirementsLongerThan500Characters_FailsValidation()
    {
        // Arrange
        var post = new PostBuilder().Build();

        // Act
        var result = post.Update(new PostRevision { Requirements = new string('r', PostErrors.MaxRequirementsLength + 1) }, acceptedGuests: 0, Now);

        // Assert
        result.Error.Code.Should().Be("posts.event.requirements_too_long");
    }

    private static Post Started() => new PostBuilder().From(Now.AddMinutes(-10), Now.AddHours(2)).Build();

    private static TimeRange Range(DateTimeOffset start, DateTimeOffset end) => TimeRange.Create(start, end).Value;
}
