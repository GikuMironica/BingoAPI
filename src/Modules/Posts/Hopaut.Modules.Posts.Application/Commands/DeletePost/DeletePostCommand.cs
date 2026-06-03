using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Posts.Application.Commands.DeletePost;

public sealed record DeletePostCommand(PostId PostId, UserId UserId) : IRequest<bool>;
