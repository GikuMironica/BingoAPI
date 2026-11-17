using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>Expected failures of the Post aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class PostErrors
{
    public const int MaxRequirementsLength = 500;

    public static Error NotFound(PostId postId) =>
        Error.NotFound("posts.post.not_found", $"Post {postId} was not found.");

    public static Error NotHost(PostId postId) =>
        Error.Forbidden("posts.post.not_host", $"Only the host of post {postId} can change it.");

    public static readonly Error EndRequired =
        Error.Rule("posts.post.end_required", "An event needs an end time.");

    public static readonly Error TooShort =
        Error.Rule("posts.post.too_short", "An event lasts at least 15 minutes.");

    public static readonly Error TooLong =
        Error.Rule("posts.post.too_long", "An event lasts less than 12 hours.");

    public static readonly Error StartsInThePast =
        Error.Rule("posts.post.starts_in_the_past", "An event cannot start more than 30 minutes in the past.");

    public static readonly Error RequirementsTooLong =
        Error.Validation("posts.event.requirements_too_long", $"The requirements can have at most {MaxRequirementsLength} characters.");

    public static Error AlreadyDisabled(PostId postId) =>
        Error.Conflict("posts.post.already_disabled", $"Post {postId} is already disabled.");

    public static Error PictureNotFound(PictureId pictureId) =>
        Error.NotFound("posts.picture.not_found", $"Picture {pictureId} does not belong to this post.");

    public static Error PictureNotPending(PictureId pictureId) =>
        Error.Conflict("posts.picture.not_pending", $"Picture {pictureId} has already been processed.");
}
