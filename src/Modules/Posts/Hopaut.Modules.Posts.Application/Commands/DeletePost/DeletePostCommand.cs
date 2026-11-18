using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.DeletePost;

/// <summary>Deletes a post; only its host or an administrator may (<see cref="Authorization.PostPolicies.MustOwnPost"/>).</summary>
public sealed record DeletePostCommand(PostId PostId) : ICommand<Result>, IPostsCommand;
