using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Queries;
using Hopaut.Modules.Posts.Application.Queries.GetMyAttendances;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class GetMyAttendancesQueryHandlerTests
{
    private static readonly DateTimeOffset Now = PostBuilder.Now;
    private static readonly UserId Guest = UserId.From("guest-1");
    private static readonly PostId[] Attended = [PostId.From(7), PostId.From(8), PostId.From(9)];

    private readonly IPostReadModel _readModel = Substitute.For<IPostReadModel>();
    private readonly IAttendanceQueries _attendance = Substitute.For<IAttendanceQueries>();
    private readonly IUserReputationProvider _reputations = Substitute.For<IUserReputationProvider>();
    private readonly GetMyAttendancesQueryHandler _handler;

    public GetMyAttendancesQueryHandlerTests()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(Guest);
        _attendance.GetAcceptedPostIdsAsync(Guest, Arg.Any<CancellationToken>()).Returns(Attended);
        _reputations.GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<UserId, UserReputationInfo>());
        _handler = new GetMyAttendancesQueryHandler(_readModel, _attendance, _reputations, currentUser, new FakeTimeProvider(Now));
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Handle_WithInvalidInputs_ReportsEachFieldAndReadsNothing()
    {
        // Act
        var result = await _handler.Handle(new GetMyAttendancesQuery("archived", "%%", 101), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("status", "cursor", "limit");
            await _attendance.DidNotReceive().GetAcceptedPostIdsAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Handle_WhenMorePostsFollow_ReturnsOnePageWithTheHostsReputationAndTheCursorOfItsLastItem()
    {
        // Arrange
        var host = UserId.From(new PostBuilder().BuildDto().UserId);
        _reputations.GetBatchAsync(Arg.Is<IEnumerable<UserId>>(hosts => hosts.SequenceEqual(new[] { host })), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<UserId, UserReputationInfo> { [host] = new(3, 4.0) });
        _readModel.GetAttendedAsync(Attended, PostListFilter.Inactive, Now, null, 3, Arg.Any<CancellationToken>())
            .Returns([Dto(9, Now.AddDays(-1)), Dto(8, Now.AddDays(-2)), Dto(7, Now.AddDays(-3))]);

        // Act
        var result = await _handler.Handle(new GetMyAttendancesQuery("inactive", null, 2), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.Id).Should().Equal(9, 8);
            PostKeyset.Decode(result.Value.NextCursor).Value.Should().Be(new PostKeyset(Now.AddDays(-2), PostId.From(8)));
            result.Value.Items.Should().OnlyContain(item => item.UserReputation == new UserReputationInfo(3, 4.0));
        }
    }

    [Fact]
    public async Task Handle_OnTheLastPage_HasNoCursorAndContinuesAfterTheGivenOne()
    {
        // Arrange
        var after = new PostKeyset(Now.AddDays(-2), PostId.From(8));
        _readModel.GetAttendedAsync(Attended, PostListFilter.Active, Now, after, PageSize.Default.Value + 1, Arg.Any<CancellationToken>())
            .Returns([Dto(7, Now.AddDays(-3))]);

        // Act
        var result = await _handler.Handle(new GetMyAttendancesQuery(null, after.Encode().Value, null), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.Id).Should().Equal(7);
            result.Value.NextCursor.Should().BeNull();
        }
    }

    [Fact]
    public async Task Handle_WithoutAttendedPosts_IsAnEmptyPageWithoutAReputationLookup()
    {
        // Arrange
        _attendance.GetAcceptedPostIdsAsync(Guest, Arg.Any<CancellationToken>()).Returns(Array.Empty<PostId>());

        // Act
        var result = await _handler.Handle(new GetMyAttendancesQuery("active", null, null), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Should().BeEmpty();
            result.Value.NextCursor.Should().BeNull();
            await _reputations.DidNotReceive().GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>());
        }
    }

    private static PostDto Dto(int id, DateTimeOffset start) => new PostBuilder().WithId(id).BuildDto() with { EventTime = start };
}
