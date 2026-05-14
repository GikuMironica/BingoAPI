using MediatR;

namespace Hopaut.Modules.Posts.Application.Commands.DeletePost;

public sealed record DeletePostCommand(int PostId, string UserId) : IRequest<bool>;
