using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Authorization;
using Hopaut.Modules.Posts.Application.Commands.DeletePost;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Domain.Events;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class DeletePostCommandHandlerTests
{
    private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
    private readonly IResourceAuthorizer _authorizer = Substitute.For<IResourceAuthorizer>();
    private readonly DeletePostCommandHandler _handler;

    public DeletePostCommandHandlerTests() =>
        _handler = new DeletePostCommandHandler(_posts, _authorizer, new FakeTimeProvider(PostBuilder.Now));

    [Fact]
    public async Task Handle_ForAMissingPost_IsNotFoundWithoutAuthorizing()
    {
        // Act
        var result = await _handler.Handle(new DeletePostCommand(PostId.From(5)), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.not_found");
            await _authorizer.DidNotReceive().IsAuthorizedAsync(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Handle_WhenTheCallerMayNotOwnThePost_IsForbiddenAndKeepsIt()
    {
        // Arrange
        var post = Existing(denied: true);

        // Act
        var result = await _handler.Handle(new DeletePostCommand(post.Id), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.not_host");
            _posts.DidNotReceive().Remove(Arg.Any<Post>());
            post.DomainEvents.OfType<PostDeletedDomainEvent>().Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_WhenAuthorized_RemovesThePostAndRaisesDeleted()
    {
        // Arrange
        var post = Existing(denied: false);

        // Act
        var result = await _handler.Handle(new DeletePostCommand(post.Id), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            _posts.Received(1).Remove(post);
            post.DomainEvents.OfType<PostDeletedDomainEvent>().Should().ContainSingle();
        }
    }

    private Post Existing(bool denied)
    {
        var post = new PostBuilder().Build();
        _posts.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _authorizer.IsAuthorizedAsync(post, PostPolicies.MustOwnPost, Arg.Any<CancellationToken>()).Returns(!denied);
        return post;
    }
}
