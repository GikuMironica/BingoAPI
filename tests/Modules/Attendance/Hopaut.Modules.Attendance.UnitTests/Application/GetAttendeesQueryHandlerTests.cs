using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Attendance.Application.Queries;
using Hopaut.Modules.Attendance.Application.Queries.GetAttendees;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using NSubstitute;
using static Hopaut.Modules.Attendance.UnitTests.AttendanceTestData;

namespace Hopaut.Modules.Attendance.UnitTests.Application;

public sealed class GetAttendeesQueryHandlerTests
{
    private readonly IAttendanceReadModel _readModel = Substitute.For<IAttendanceReadModel>();
    private readonly IPostAttendanceTermsQueries _posts = Substitute.For<IPostAttendanceTermsQueries>();
    private readonly IResourceAuthorizer _authorizer = Substitute.For<IResourceAuthorizer>();
    private readonly GetAttendeesQueryHandler _handler;

    public GetAttendeesQueryHandlerTests() => _handler = new GetAttendeesQueryHandler(_readModel, _posts, _authorizer);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void AttendeeKeyset_EncodedAndDecoded_RoundTrips()
    {
        // Arrange
        var keyset = new AttendeeKeyset(Now, ParticipationId.From(12));

        // Act
        var decoded = AttendeeKeyset.Decode(keyset.Encode().Value);

        // Assert
        decoded.Value.Should().Be(keyset);
    }

    [Theory]
    [InlineData("not base64!")]
    [InlineData("MTIzNA")] // "1234": no separator
    [InlineData("MTIzNDow")] // "1234:0": no participation has ID 0
    [InlineData("eDox")] // "x:1": no tick count
    public void AttendeeKeyset_DecodingAnUnreadableCursor_IsAnInvalidCursor(string value)
    {
        // Act
        var decoded = AttendeeKeyset.Decode(value);

        // Assert
        decoded.Error.Code.Should().Be("shared.cursor.invalid");
    }

    [Theory]
    [InlineData(AttendanceStatus.Pending, "pending")]
    [InlineData(AttendanceStatus.Accepted, "accepted")]
    [InlineData(AttendanceStatus.Rejected, "rejected")]
    [InlineData(AttendanceStatus.Cancelled, "cancelled")]
    public void StatusName_IsTheLowerCaseWireName(AttendanceStatus status, string expected) =>
        AttendeeLists.StatusName(status).Should().Be(expected);

    [Fact]
    public async Task Handle_WithInvalidInputs_ReportsEachFieldAndReadsNothing()
    {
        // Act
        var result = await _handler.Handle(new GetAttendeesQuery(Post, "rejected", "%%", 0), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("status", "cursor", "limit");
            await _posts.DidNotReceive().GetAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Handle_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await _handler.Handle(new GetAttendeesQuery(Post, null, null, null), Ct);

        // Assert
        result.Error.Should().Be(ParticipationErrors.PostNotFound(Post));
    }

    [Fact]
    public async Task Handle_PendingRequestsForSomeoneOtherThanTheHost_IsForbiddenAndReadsNothing()
    {
        // Arrange
        PostExists(callerHosts: false);

        // Act
        var result = await _handler.Handle(new GetAttendeesQuery(Post, "Pending", null, null), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(ParticipationErrors.NotPostHost(Post));
            await _readModel.DidNotReceive().GetAttendeesAsync(
                Arg.Any<PostId>(), Arg.Any<AttendanceStatus>(), Arg.Any<AttendeeKeyset?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Handle_AcceptedAttendeesForAnyCaller_ReturnsOnePageAndTheCursorOfItsLastItem()
    {
        // Arrange
        PostExists(callerHosts: false);
        _readModel.GetAttendeesAsync(Post, AttendanceStatus.Accepted, null, 3, Arg.Any<CancellationToken>())
            .Returns([Attendee(1, Now), Attendee(2, Now.AddMinutes(1)), Attendee(3, Now.AddMinutes(2))]);

        // Act
        var result = await _handler.Handle(new GetAttendeesQuery(Post, null, null, 2), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.ParticipationId.Value).Should().Equal(1, 2);
            AttendeeKeyset.Decode(result.Value.NextCursor).Value.Should().Be(new AttendeeKeyset(Now.AddMinutes(1), ParticipationId.From(2)));
            await _authorizer.DidNotReceive().IsAuthorizedAsync(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Handle_PendingRequestsForTheHostOnTheLastPage_HaveNoCursorAndContinueAfterTheGivenOne()
    {
        // Arrange
        PostExists(callerHosts: true);
        var after = new AttendeeKeyset(Now, ParticipationId.From(4));
        _readModel.GetAttendeesAsync(Post, AttendanceStatus.Pending, after, PageSize.Default.Value + 1, Arg.Any<CancellationToken>())
            .Returns([Attendee(5, Now.AddMinutes(3))]);

        // Act
        var result = await _handler.Handle(new GetAttendeesQuery(Post, "pending", after.Encode().Value, null), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.ParticipationId.Value).Should().Equal(5);
            result.Value.NextCursor.Should().BeNull();
        }
    }

    private void PostExists(bool callerHosts)
    {
        var host = new PostHost(Post, Host);
        _posts.GetAsync(Post, Arg.Any<CancellationToken>())
            .Returns(new PostAttendanceTerms(host, IsListed: true, EndsAt: Now.AddHours(6), HostApprovesAttendees: true, Slots: null));
        _authorizer.IsAuthorizedAsync(host, AttendancePolicies.MustHostPost, Arg.Any<CancellationToken>()).Returns(callerHosts);
    }

    private static Attendee Attendee(int id, DateTimeOffset requestedAt) =>
        new(ParticipationId.From(id), UserId.From($"guest-{id}"), "accepted", requestedAt, RespondedAt: null);
}
