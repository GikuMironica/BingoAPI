using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.DisablePost;

/// <summary>Takes a post off every list (the legacy admin "disable"); the endpoint requires the Admin policy.</summary>
public sealed record DisablePostCommand(PostId PostId) : ICommand<Result>, IPostsCommand;
