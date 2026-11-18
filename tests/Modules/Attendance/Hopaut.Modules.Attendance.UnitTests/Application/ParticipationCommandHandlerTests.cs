using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;
using Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Attendance.Application.Queries;
using Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Attendance.Domain.Events;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using static Hopaut.Modules.Attendance.UnitTests.AttendanceTestData;

namespace Hopaut.Modules.Attendance.UnitTests.Application;

public sealed class ParticipationCommandHandlerTests
{
    private readonly IAttendanceRepository _participations = Substitute.For<IAttendanceRepository>();
    private readonly IPostAttendanceTermsQueries _posts = Substitute.For<IPostAttendanceTermsQueries>();
    private readonly IResourceAuthorizer _authorizer = Substitute.For<IResourceAuthorizer>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly FakeTimeProvider _time = new(Now);

    public ParticipationCommandHandlerTests()
    {
        _currentUser.Id.Returns(Guest);
        _participations.NextIdAsync(Arg.Any<CancellationToken>()).Returns(Id);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Request_ForAMissingPost_IsNotFoundAndTakesNoId()
    {
        // Act
        var result = await RequestHandler().Handle(new RequestAttendanceCommand(Post), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.PostNotFound(Post));
            await _participations.DidNotReceive().NextIdAsync(Arg.Any<CancellationToken>());
            _participations.DidNotReceive().Add(Arg.Any<Participation>());
        }
    }

    [Fact]
    public async Task Request_ForTheFirstTime_AddsTheCallersRequestUnderASequenceId()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null);

        // Act
        var result = await RequestHandler().Handle(new RequestAttendanceCommand(Post), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            _participations.Received(1).Add(Arg.Is<Participation>(p =>
                p.Id == Id && p.PostId == Post && p.UserId == Guest && p.Status == AttendanceStatus.Pending));
            await _participations.DidNotReceive().CountAcceptedAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Request_ForAFullPost_CountsTheAcceptedAttendeesAndIsRefused()
    {
        // Arrange
        PostWith(hostApproves: false, slots: 4);
        _participations.CountAcceptedAsync(Post, Arg.Any<CancellationToken>()).Returns(4);

        // Act
        var result = await RequestHandler().Handle(new RequestAttendanceCommand(Post), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NoSlotsLeft(Post));
            _participations.DidNotReceive().Add(Arg.Any<Participation>());
        }
    }

    [Fact]
    public async Task Request_ByTheHost_IsRefused()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null);
        _currentUser.Id.Returns(Host);

        // Act
        var result = await RequestHandler().Handle(new RequestAttendanceCommand(Post), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.OwnPost(Post));
            _participations.DidNotReceive().Add(Arg.Any<Participation>());
        }
    }

    [Fact]
    public async Task Request_AfterACancellation_OpensTheExistingParticipation()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null);
        var participation = Existing(AttendanceStatus.Cancelled);

        // Act
        var result = await RequestHandler().Handle(new RequestAttendanceCommand(Post), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Pending);
            await _participations.DidNotReceive().NextIdAsync(Arg.Any<CancellationToken>());
            _participations.DidNotReceive().Add(Arg.Any<Participation>());
        }
    }

    [Fact]
    public async Task Request_WhileARequestIsOpen_IsAlreadyRequested()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null);
        Existing(AttendanceStatus.Pending);

        // Act
        var result = await RequestHandler().Handle(new RequestAttendanceCommand(Post), Ct);

        // Assert
        result.Error.Should().Be(ParticipationErrors.AlreadyRequested(Post, Guest));
    }

    [Fact]
    public async Task Accept_ForAMissingPost_IsNotFoundWithoutAuthorizing()
    {
        // Act
        var result = await AcceptHandler().Handle(new AcceptAttendanceCommand(Post, Guest), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.PostNotFound(Post));
            await _authorizer.DidNotReceive().IsAuthorizedAsync(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Accept_ByACallerWhoDoesNotHostThePost_IsForbiddenAndKeepsTheRequestPending()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null, callerHosts: false);
        var participation = Existing(AttendanceStatus.Pending);

        // Act
        var result = await AcceptHandler().Handle(new AcceptAttendanceCommand(Post, Guest), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NotPostHost(Post));
            participation.Status.Should().Be(AttendanceStatus.Pending);
        }
    }

    [Fact]
    public async Task Accept_WithoutARequest_IsNotFound()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null);

        // Act
        var result = await AcceptHandler().Handle(new AcceptAttendanceCommand(Post, Guest), Ct);

        // Assert
        result.Error.Should().Be(ParticipationErrors.NotFound(Post, Guest));
    }

    [Fact]
    public async Task Accept_ByTheHost_AcceptsThePendingRequest()
    {
        // Arrange
        PostWith(hostApproves: true, slots: 2);
        _participations.CountAcceptedAsync(Post, Arg.Any<CancellationToken>()).Returns(1);
        var participation = Existing(AttendanceStatus.Pending);

        // Act
        var result = await AcceptHandler().Handle(new AcceptAttendanceCommand(Post, Guest), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Accepted);
            participation.DomainEvents.OfType<AttendanceAcceptedDomainEvent>().Should().ContainSingle();
        }
    }

    [Fact]
    public async Task Accept_WhenEverySlotIsTaken_IsRefused()
    {
        // Arrange
        PostWith(hostApproves: true, slots: 2);
        _participations.CountAcceptedAsync(Post, Arg.Any<CancellationToken>()).Returns(2);
        Existing(AttendanceStatus.Pending);

        // Act
        var result = await AcceptHandler().Handle(new AcceptAttendanceCommand(Post, Guest), Ct);

        // Assert
        result.Error.Should().Be(ParticipationErrors.NoSlotsLeft(Post));
    }

    [Fact]
    public async Task Reject_ByACallerWhoDoesNotHostThePost_IsForbidden()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null, callerHosts: false);
        var participation = Existing(AttendanceStatus.Accepted);

        // Act
        var result = await RejectHandler().Handle(new RejectAttendanceCommand(Post, Guest), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NotPostHost(Post));
            participation.Status.Should().Be(AttendanceStatus.Accepted);
        }
    }

    [Fact]
    public async Task Reject_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await RejectHandler().Handle(new RejectAttendanceCommand(Post, Guest), Ct);

        // Assert
        result.Error.Should().Be(ParticipationErrors.PostNotFound(Post));
    }

    [Fact]
    public async Task Reject_WithoutARequest_IsNotFound()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null);

        // Act
        var result = await RejectHandler().Handle(new RejectAttendanceCommand(Post, Guest), Ct);

        // Assert
        result.Error.Should().Be(ParticipationErrors.NotFound(Post, Guest));
    }

    [Fact]
    public async Task Reject_ByTheHost_RemovesAnAcceptedAttendee()
    {
        // Arrange
        PostWith(hostApproves: true, slots: null);
        var participation = Existing(AttendanceStatus.Accepted);

        // Act
        var result = await RejectHandler().Handle(new RejectAttendanceCommand(Post, Guest), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Rejected);
        }
    }

    [Fact]
    public async Task Cancel_WithoutARequest_IsNotFoundForTheCaller()
    {
        // Act
        var result = await CancelHandler().Handle(new CancelAttendanceCommand(Post), Ct);

        // Assert
        result.Error.Should().Be(ParticipationErrors.NotFound(Post, Guest));
    }

    [Fact]
    public async Task Cancel_TheCallersAttendance_CancelsIt()
    {
        // Arrange
        var participation = Existing(AttendanceStatus.Accepted);

        // Act
        var result = await CancelHandler().Handle(new CancelAttendanceCommand(Post), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            participation.Status.Should().Be(AttendanceStatus.Cancelled);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsUserAttending_AnswersForTheCaller(bool accepted)
    {
        // Arrange
        var readModel = Substitute.For<IAttendanceReadModel>();
        readModel.IsAcceptedAsync(Post, Guest, Arg.Any<CancellationToken>()).Returns(accepted);

        // Act
        var result = await new IsUserAttendingQueryHandler(readModel, _currentUser).Handle(new IsUserAttendingQuery(Post), Ct);

        // Assert
        result.Value.Should().Be(accepted);
    }

    private RequestAttendanceCommandHandler RequestHandler() => new(_participations, _posts, _currentUser, _time);

    private AcceptAttendanceCommandHandler AcceptHandler() => new(_participations, _posts, _authorizer, _time);

    private RejectAttendanceCommandHandler RejectHandler() => new(_participations, _posts, _authorizer, _time);

    private CancelAttendanceCommandHandler CancelHandler() => new(_participations, _currentUser, _time);

    private void PostWith(bool hostApproves, int? slots, bool callerHosts = true)
    {
        var host = new PostHost(Post, Host);
        _posts.GetAsync(Post, Arg.Any<CancellationToken>())
            .Returns(new PostAttendanceTerms(host, IsListed: true, EndsAt: Now.AddHours(6), hostApproves, slots));
        _authorizer.IsAuthorizedAsync(host, AttendancePolicies.MustHostPost, Arg.Any<CancellationToken>()).Returns(callerHosts);
    }

    private Participation Existing(AttendanceStatus status)
    {
        var participation = ParticipationIn(status);
        _participations.GetAsync(Post, Guest, Arg.Any<CancellationToken>()).Returns(participation);
        return participation;
    }
}
