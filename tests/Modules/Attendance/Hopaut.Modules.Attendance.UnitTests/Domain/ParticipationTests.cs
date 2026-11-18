using AwesomeAssertions.Execution;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Attendance.Domain.Events;
using Hopaut.SharedKernel;
using static Hopaut.Modules.Attendance.UnitTests.AttendanceTestData;

namespace Hopaut.Modules.Attendance.UnitTests.Domain;

public sealed class ParticipationTests
{
    [Fact]
    public void Request_ForAHouseParty_WaitsForTheHost()
    {
        // Act
        var participation = Participation.Request(Id, HouseParty(), Guest, acceptedAttendees: 0, Now).Value;

        // Assert
        using (new AssertionScope())
        {
            participation.Id.Should().Be(Id);
            participation.PostId.Should().Be(Post);
            participation.UserId.Should().Be(Guest);
            participation.Status.Should().Be(AttendanceStatus.Pending);
            participation.IsActive.Should().BeTrue();
            participation.RequestedAt.Should().Be(Now);
            participation.RespondedAt.Should().BeNull();
            participation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new AttendanceRequestedDomainEvent(Now, Id, Post, Guest, AwaitsApproval: true), options => options.Excluding(e => e.EventId));
        }
    }

    [Fact]
    public void Request_ForAnEventOtherThanAHouseParty_IsAcceptedAtOnce()
    {
        // Act
        var participation = Participation.Request(Id, OpenEvent(), Guest, acceptedAttendees: 0, Now).Value;

        // Assert
        using (new AssertionScope())
        {
            participation.Status.Should().Be(AttendanceStatus.Accepted);
            participation.RespondedAt.Should().Be(Now);
            participation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new AttendanceRequestedDomainEvent(Now, Id, Post, Guest, AwaitsApproval: false), options => options.Excluding(e => e.EventId));
        }
    }

    [Fact]
    public void Request_ByTheHost_IsRefused()
    {
        // Act
        var result = Participation.Request(Id, HouseParty(), Host, acceptedAttendees: 0, Now);

        // Assert
        result.Error.Should().Be(ParticipationErrors.OwnPost(Post));
    }

    [Fact]
    public void Request_ForADisabledPost_IsRefused()
    {
        // Act
        var result = Participation.Request(Id, HouseParty() with { IsListed = false }, Guest, acceptedAttendees: 0, Now);

        // Assert
        result.Error.Code.Should().Be("attendance.participation.post_disabled");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Request_WhenThePostHasEnded_IsRefused(int minutesFromNow)
    {
        // Arrange
        var terms = OpenEvent() with { EndsAt = Now.AddMinutes(minutesFromNow) };

        // Act
        var result = Participation.Request(Id, terms, Guest, acceptedAttendees: 0, Now);

        // Assert
        result.Error.Should().Be(ParticipationErrors.PostEnded(Post));
    }

    [Fact]
    public void Request_ForAPostWithoutAnEnd_Succeeds()
    {
        // Act
        var result = Participation.Request(Id, OpenEvent() with { EndsAt = null }, Guest, acceptedAttendees: 0, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Request_WhenEverySlotIsTaken_IsRefused(bool hostApproves)
    {
        // Arrange
        var terms = hostApproves ? HouseParty(slots: 3) : OpenEvent(slots: 3);

        // Act
        var result = Participation.Request(Id, terms, Guest, acceptedAttendees: 3, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NoSlotsLeft(Post));
            result.Error.Type.Should().Be(ErrorType.Rule);
        }
    }

    [Fact]
    public void Request_WithOneSlotLeft_Succeeds()
    {
        // Act
        var result = Participation.Request(Id, OpenEvent(slots: 3), Guest, acceptedAttendees: 2, Now);

        // Assert
        result.Value.Status.Should().Be(AttendanceStatus.Accepted);
    }

    [Fact]
    public void Request_WithoutAnId_IsAProgrammingError()
    {
        // Act
        var act = () => Participation.Request(default, HouseParty(), Guest, acceptedAttendees: 0, Now);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(AttendanceStatus.Rejected)]
    [InlineData(AttendanceStatus.Cancelled)]
    public void RequestAgain_AfterARejectionOrACancellation_OpensTheSameParticipation(AttendanceStatus status)
    {
        // Arrange
        var participation = ParticipationIn(status);
        var later = Now.AddMinutes(10);

        // Act
        var result = participation.RequestAgain(HouseParty(), acceptedAttendees: 0, later);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Pending);
            participation.RequestedAt.Should().Be(later);
            participation.RespondedAt.Should().BeNull();
            participation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new AttendanceRequestedDomainEvent(later, Id, Post, Guest, AwaitsApproval: true), options => options.Excluding(e => e.EventId));
        }
    }

    [Theory]
    [InlineData(AttendanceStatus.Pending)]
    [InlineData(AttendanceStatus.Accepted)]
    public void RequestAgain_WhileTheParticipationIsActive_IsAlreadyRequested(AttendanceStatus status)
    {
        // Arrange
        var participation = ParticipationIn(status);

        // Act
        var result = participation.RequestAgain(HouseParty(), acceptedAttendees: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.AlreadyRequested(Post, Guest));
            participation.Status.Should().Be(status);
            participation.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void RequestAgain_FollowsTheRulesOfAFirstRequest()
    {
        // Arrange
        var participation = ParticipationIn(AttendanceStatus.Cancelled);

        // Act
        var result = participation.RequestAgain(HouseParty(slots: 1), acceptedAttendees: 1, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NoSlotsLeft(Post));
            participation.Status.Should().Be(AttendanceStatus.Cancelled);
        }
    }

    [Fact]
    public void RequestAgain_WithTheTermsOfAnotherPost_IsAProgrammingError()
    {
        // Arrange
        var participation = ParticipationIn(AttendanceStatus.Cancelled);

        // Act
        var act = () => participation.RequestAgain(HouseParty() with { PostId = PostId.From(8) }, acceptedAttendees: 0, Now);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Accept_APendingRequest_AcceptsIt()
    {
        // Arrange
        var participation = ParticipationIn(AttendanceStatus.Pending);
        var later = Now.AddMinutes(5);

        // Act
        var result = participation.Accept(HouseParty(slots: 2), acceptedAttendees: 1, later);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Accepted);
            participation.RespondedAt.Should().Be(later);
            participation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new AttendanceAcceptedDomainEvent(later, Id, Post, Guest), options => options.Excluding(e => e.EventId));
        }
    }

    [Theory]
    [InlineData(AttendanceStatus.Accepted)]
    [InlineData(AttendanceStatus.Rejected)]
    [InlineData(AttendanceStatus.Cancelled)]
    public void Accept_WhenNotPending_IsRefused(AttendanceStatus status)
    {
        // Arrange
        var participation = ParticipationIn(status);

        // Act
        var result = participation.Accept(HouseParty(), acceptedAttendees: 0, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NotPending(Id));
            participation.Status.Should().Be(status);
        }
    }

    [Fact]
    public void Accept_WhenEverySlotIsTaken_KeepsTheRequestPending()
    {
        // Arrange
        var participation = ParticipationIn(AttendanceStatus.Pending);

        // Act
        var result = participation.Accept(HouseParty(slots: 2), acceptedAttendees: 2, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NoSlotsLeft(Post));
            participation.Status.Should().Be(AttendanceStatus.Pending);
            participation.DomainEvents.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData(AttendanceStatus.Pending)]
    [InlineData(AttendanceStatus.Accepted)]
    public void Reject_AnActiveParticipation_RejectsIt(AttendanceStatus status)
    {
        // Arrange
        var participation = ParticipationIn(status);
        var later = Now.AddMinutes(5);

        // Act
        var result = participation.Reject(later);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Rejected);
            participation.IsActive.Should().BeFalse();
            participation.RespondedAt.Should().Be(later);
            participation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new AttendanceRejectedDomainEvent(later, Id, Post, Guest), options => options.Excluding(e => e.EventId));
        }
    }

    [Theory]
    [InlineData(AttendanceStatus.Rejected)]
    [InlineData(AttendanceStatus.Cancelled)]
    public void Reject_AnInactiveParticipation_IsRefused(AttendanceStatus status)
    {
        // Arrange
        var participation = ParticipationIn(status);

        // Act
        var result = participation.Reject(Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NotActive(Id));
            participation.Status.Should().Be(status);
        }
    }

    [Theory]
    [InlineData(AttendanceStatus.Pending)]
    [InlineData(AttendanceStatus.Accepted)]
    public void Cancel_AnActiveParticipation_CancelsIt(AttendanceStatus status)
    {
        // Arrange
        var participation = ParticipationIn(status);
        var later = Now.AddMinutes(5);

        // Act
        var result = participation.Cancel(later);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Cancelled);
            participation.RespondedAt.Should().Be(later);
            participation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new AttendanceCancelledDomainEvent(later, Id, Post, Guest), options => options.Excluding(e => e.EventId));
        }
    }

    [Fact]
    public void Cancel_ACancelledParticipation_IsAlreadyCancelled()
    {
        // Arrange
        var participation = ParticipationIn(AttendanceStatus.Cancelled);

        // Act
        var result = participation.Cancel(Now);

        // Assert
        result.Error.Should().Be(ParticipationErrors.AlreadyCancelled(Id));
    }

    [Fact]
    public void Cancel_ARejectedParticipation_IsRefused()
    {
        // Arrange
        var participation = ParticipationIn(AttendanceStatus.Rejected);

        // Act
        var result = participation.Cancel(Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NotActive(Id));
            participation.Status.Should().Be(AttendanceStatus.Rejected);
        }
    }
}
