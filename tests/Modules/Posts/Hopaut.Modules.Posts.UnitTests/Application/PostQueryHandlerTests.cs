using AwesomeAssertions.Execution;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Queries;
using Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;
using Hopaut.Modules.Posts.Application.Queries.GetPostById;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class PostQueryHandlerTests
{
    private static readonly DateTimeOffset Now = PostBuilder.Now;

    private readonly IPostReadModel _readModel = Substitute.For<IPostReadModel>();
    private readonly IUserReputationProvider _reputations = Substitute.For<IUserReputationProvider>();
    private readonly GetNearbyPostsQueryHandler _nearby;

    public PostQueryHandlerTests()
    {
        _reputations.GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<UserId, UserReputationInfo>());
        _nearby = new GetNearbyPostsQueryHandler(_readModel, _reputations, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task GetById_ForAnExistingPost_ReturnsTheReadModelsPost()
    {
        // Arrange
        var post = new PostBuilder().BuildDto();
        _readModel.GetByIdAsync(PostId.From(post.Id), Arg.Any<CancellationToken>()).Returns(post);

        // Act
        var result = await new GetPostByIdQueryHandler(_readModel).Handle(new GetPostByIdQuery(PostId.From(post.Id)), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Should().Be(post);
            result.Value.ActiveFlag.Should().Be(1);
            result.Value.Event.Slots.Should().Be(20);
            result.Value.Location.Latitude.Should().Be(52.52);
        }
    }

    [Fact]
    public async Task GetById_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await new GetPostByIdQueryHandler(_readModel).Handle(new GetPostByIdQuery(PostId.From(9)), TestContext.Current.CancellationToken);

        // Assert
        result.Error.Code.Should().Be("posts.post.not_found");
    }

    [Fact]
    public async Task GetNearby_AttachesEachHostsReputationFromOneBatchedLookup()
    {
        // Arrange
        Nearby(null, PageSize.Default.Value + 1, (new PostBuilder().WithId(1).HostedBy("host-a").BuildDto(), 120), (new PostBuilder().WithId(2).HostedBy("host-b").BuildDto(), 480));
        _reputations.GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<UserId, UserReputationInfo> { [UserId.From("host-a")] = new(3, 4.5) });

        // Act
        var result = await _nearby.Handle(new GetNearbyPostsQuery(13.4, 52.5), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(p => p.UserReputation).Should().Equal(new UserReputationInfo(3, 4.5), null);
            result.Value.NextCursor.Should().BeNull();
            await _reputations.Received(1).GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetNearby_WhenMorePostsFollow_ReturnsTheCursorOfTheLastItemsDistance()
    {
        // Arrange
        Nearby(null, 2, (new PostBuilder().WithId(1).BuildDto(), 120.5), (new PostBuilder().WithId(2).BuildDto(), 480.25));

        // Act
        var result = await _nearby.Handle(new GetNearbyPostsQuery(13.4, 52.5, Limit: 1), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(p => p.Id).Should().Equal(1);
            NearbyKeyset.Decode(result.Value.NextCursor).Value.Should().Be(new NearbyKeyset(120.5, PostId.From(1)));
        }
    }

    [Fact]
    public async Task GetNearby_WithACursor_ContinuesAfterIt()
    {
        // Arrange
        var after = new NearbyKeyset(120.5, PostId.From(1));
        Nearby(after, PageSize.Default.Value + 1, (new PostBuilder().WithId(2).BuildDto(), 480.25));

        // Act
        var result = await _nearby.Handle(new GetNearbyPostsQuery(13.4, 52.5, Cursor: after.Encode().Value), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Items.Select(p => p.Id).Should().Equal(2);
    }

    [Theory]
    [InlineData(999, 20, "radius")]
    [InlineData(15_001, 20, "radius")]
    [InlineData(double.NaN, 20, "radius")]
    [InlineData(10_000, 101, "limit")]
    [InlineData(10_000, 0, "limit")]
    public async Task GetNearby_OutsideTheBounds_ReportsTheFieldAndReadsNothing(double radius, int limit, string field)
    {
        // Act
        var result = await _nearby.Handle(new GetNearbyPostsQuery(13.4, 52.5, radius, limit), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().Equal(field);
            await _readModel.DidNotReceive().GetNearbyAsync(
                Arg.Any<Coordinates>(), Arg.Any<RadiusMeters>(), Arg.Any<DateTimeOffset>(), Arg.Any<NearbyKeyset?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetNearby_WithInvalidCoordinatesAndCursor_ReportsBoth()
    {
        // Act
        var result = await _nearby.Handle(new GetNearbyPostsQuery(181, 52.5, Cursor: "%%"), TestContext.Current.CancellationToken);

        // Assert
        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("location", "cursor");
    }

    [Fact]
    public void NearbyKeyset_EncodedAndDecoded_KeepsTheExactDistance()
    {
        // Arrange
        var keyset = new NearbyKeyset(1234.5678901234567, PostId.From(77));

        // Act
        var decoded = NearbyKeyset.Decode(keyset.Encode().Value);

        // Assert
        decoded.Value.Should().Be(keyset);
    }

    private void Nearby(NearbyKeyset? after, int count, params (PostDto Post, double Distance)[] posts) =>
        _readModel.GetNearbyAsync(Arg.Any<Coordinates>(), Arg.Any<RadiusMeters>(), Now, after, count, Arg.Any<CancellationToken>())
            .Returns(posts.Select(post => new NearbyPost(post.Post, post.Distance)).ToList());
}
