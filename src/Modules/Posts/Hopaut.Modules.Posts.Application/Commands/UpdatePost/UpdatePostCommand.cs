using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.UpdatePost;

/// <summary>
/// The host's partial edit of a post (the legacy update): a null part keeps its value; a location replaces the whole
/// location. Only the host or an administrator may (<see cref="Authorization.PostPolicies.MustOwnPost"/>).
/// </summary>
public sealed record UpdatePostCommand(
    PostId PostId,
    DateTimeOffset? EventTime,
    DateTimeOffset? EndTime,
    CreatePostLocationInput? Location,
    UpdatePostEventInput? Event,
    IReadOnlyList<string>? TagNames) : ICommand<Result>, IPostsCommand;

/// <summary>Event parts of an edit; the fee keeps today's wire shape (price + legacy currency index, D-026).</summary>
public sealed record UpdatePostEventInput(string? Title, string? Description, string? Requirements, int? Slots, double? EntrancePrice, int? Currency);
