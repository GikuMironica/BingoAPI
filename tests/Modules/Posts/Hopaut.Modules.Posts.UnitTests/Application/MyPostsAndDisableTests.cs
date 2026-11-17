using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Commands.DisablePost;
using Hopaut.Modules.Posts.Application.Queries;
using Hopaut.Modules.Posts.Application.Queries.GetMyPosts;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class MyPostsAndDisableTests
{
    private static readonly DateTimeOffset Now = PostBuilder.Now;
    private static readonly UserId Host = UserId.From("host-1");

    private readonly IPostReadModel _readModel = Substitute.For<IPostReadModel>();
    private readonly IUserReputationProvider _reputations = Substitute.For<IUserReputationProvider>();
    private readonly GetMyPostsQueryHandler _handler;

    public MyPostsAndDisableTests()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(Host);
        _reputations.GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<UserId, UserReputationInfo> { [Host] = new(2, 4.5) });
        _handler = new GetMyPostsQueryHandler(_readModel, _reputations, currentUser, new FakeTimeProvider(Now));
    }

    [Fact]
    public void PostKeyset_EncodedAndDecoded_RoundTrips()
    {
        // Arrange
        var keyset = new PostKeyset(new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero), PostId.From(42));

        // Act
        var decoded = PostKeyset.Decode(keyset.Encode().Value);

        // Assert
        decoded.Value.Should().Be(keyset);
    }

    [Theory]
    [InlineData("not base64!")]
    [InlineData("MTIzNA")] // "1234": no separator
    [InlineData("MTIzNDow")] // "1234:0": no post has ID 0
    [InlineData("eDox")] // "x:1": no tick count
    public void PostKeyset_DecodingAnUnreadableCursor_IsAnInvalidCursor(string value)
    {
        // Act
        var decoded = PostKeyset.Decode(value);

        // Assert
        decoded.Error.Code.Should().Be("shared.cursor.invalid");
    }

    [Fact]
    public async Task GetMyPosts_WithInvalidInputs_ReportsEachFieldAndReadsNothing()
    {
        // Act
        var result = await _handler.Handle(new GetMyPostsQuery("archived", "%%", 101), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("status", "cursor", "limit");
            await _readModel.DidNotReceive().GetHostedAsync(Arg.Any<UserId>(), Arg.Any<HostedPostsFilter>(), Arg.Any<DateTimeOffset>(), Arg.Any<PostKeyset?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetMyPosts_WhenMorePostsFollow_ReturnsOnePageAndTheCursorOfItsLastItem()
    {
        // Arrange
        _readModel.GetHostedAsync(Host, HostedPostsFilter.Inactive, Now, null, 3, Arg.Any<CancellationToken>())
            .Returns([Dto(9, Now.AddDays(-1)), Dto(8, Now.AddDays(-2)), Dto(7, Now.AddDays(-3))]);

        // Act
        var result = await _handler.Handle(new GetMyPostsQuery("Inactive", null, 2), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.Id).Should().Equal(9, 8);
            PostKeyset.Decode(result.Value.NextCursor).Value.Should().Be(new PostKeyset(Now.AddDays(-2), PostId.From(8)));
            result.Value.Items.Should().OnlyContain(item => item.UserReputation == new UserReputationInfo(2, 4.5));
        }
    }

    [Fact]
    public async Task GetMyPosts_OnTheLastPage_HasNoCursorAndContinuesAfterTheGivenOne()
    {
        // Arrange
        var after = new PostKeyset(Now.AddDays(-2), PostId.From(8));
        _readModel.GetHostedAsync(Host, HostedPostsFilter.Active, Now, after, PageSize.Default.Value + 1, Arg.Any<CancellationToken>())
            .Returns([Dto(7, Now.AddDays(-3))]);

        // Act
        var result = await _handler.Handle(new GetMyPostsQuery(null, after.Encode().Value, null), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.Id).Should().Equal(7);
            result.Value.NextCursor.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetMyPosts_WithoutPosts_DoesNotLookUpTheReputation()
    {
        // Act
        var result = await _handler.Handle(new GetMyPostsQuery("active", null, null), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Should().BeEmpty();
            await _reputations.DidNotReceive().GetBatchAsync(Arg.Any<IEnumerable<UserId>>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task DisablePost_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await new DisablePostCommandHandler(Substitute.For<IPostRepository>())
            .Handle(new DisablePostCommand(PostId.From(4)), TestContext.Current.CancellationToken);

        // Assert
        result.Error.Code.Should().Be("posts.post.not_found");
    }

    [Fact]
    public async Task DisablePost_TwiceInARow_DisablesThenConflicts()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var posts = Substitute.For<IPostRepository>();
        posts.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        var handler = new DisablePostCommandHandler(posts);

        // Act
        var first = await handler.Handle(new DisablePostCommand(post.Id), TestContext.Current.CancellationToken);
        var second = await handler.Handle(new DisablePostCommand(post.Id), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            first.IsSuccess.Should().BeTrue();
            post.Status.Should().Be(PostStatus.Disabled);
            second.Error.Code.Should().Be("posts.post.already_disabled");
        }
    }

    private static PostDto Dto(int id, DateTimeOffset start) => new PostBuilder().WithId(id).BuildDto() with { EventTime = start };
}
