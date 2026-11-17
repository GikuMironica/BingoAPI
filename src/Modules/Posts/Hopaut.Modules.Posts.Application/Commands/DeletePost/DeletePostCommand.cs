using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.DeletePost;

public sealed record DeletePostCommand(PostId PostId, UserId UserId) : ICommand<Result>, IPostsCommand;
