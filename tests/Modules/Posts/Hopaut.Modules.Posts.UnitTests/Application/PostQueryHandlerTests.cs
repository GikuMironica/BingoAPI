using AwesomeAssertions.Execution;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;
using Hopaut.Modules.Posts.Application.Queries.GetPostById;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using NSubstitute;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class PostQueryHandlerTests
{
    private readonly IPostRepository _posts = Substitute.For<IPostRepository>();

    [Fact]
    public async Task GetById_ForAnExistingPost_MapsValueObjectsToTodaysWireShape()
    {
        // Arrange
        var post = new PostBuilder().Build();
        _posts.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        var result = await new GetPostByIdQueryHandler(_posts).Handle(new GetPostByIdQuery(post.Id), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.ActiveFlag.Should().Be(1);
            result.Value.EventTime.Should().Be(post.Schedule.Start);
            result.Value.EndTime.Should().Be(post.Schedule.End);
            result.Value.Event.Title.Should().Be("Garden party");
            result.Value.Event.Slots.Should().Be(20);
            result.Value.Location.Latitude.Should().Be(52.52);
        }
    }

    [Fact]
    public async Task GetById_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await new GetPostByIdQueryHandler(_posts).Handle(new GetPostByIdQuery(PostId.From(9)), TestContext.Current.CancellationToken);

        // Assert
        result.Error.Code.Should().Be("posts.post.not_found");
    }

    [Fact]
    public async Task GetNearby_AttachesEachHostsReputationFromOneBatchedLookup()
    {
        // Arrange
        var first = new PostBuilder().WithId(1).HostedBy("host-a").Build();
        var second = new PostBuilder().WithId(2).HostedBy("host-b").Build();
        _posts.GetNearbyAsync(13.4, 52.5, 10_000, 50, Arg.Any<CancellationToken>()).Returns([first, second]);
        var reputations = Substitute.For<IUserReputationProvider>();
        reputations.GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<UserId, UserReputationInfo> { [UserId.From("host-a")] = new(3, 4.5) });

        // Act
        var result = await new GetNearbyPostsQueryHandler(_posts, reputations)
            .Handle(new GetNearbyPostsQuery(13.4, 52.5), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Select(p => p.UserReputation).Should().Equal(new UserReputationInfo(3, 4.5), null);
            await reputations.Received(1).GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>());
        }
    }
}
