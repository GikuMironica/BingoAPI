using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain;

/// <summary>Expected failures of the Rating aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class RatingErrors
{
    public static Error NotFound(RatingId ratingId) =>
        Error.NotFound("ratings.rating.not_found", $"Rating {ratingId} was not found.");

    public static Error NotRater(RatingId ratingId) =>
        Error.Forbidden("ratings.rating.not_rater", $"Only the author of rating {ratingId} or an administrator can delete it.");

    public static Error PostNotFound(PostId postId) =>
        Error.NotFound("ratings.rating.post_not_found", $"Post {postId} was not found.");

    public static Error SelfRating(PostId postId) =>
        Error.Rule("ratings.rating.self_rating", $"The host of post {postId} cannot rate themselves.");

    public static Error NotAttendee(PostId postId) =>
        Error.Forbidden("ratings.rating.not_attendee", $"Only accepted attendees of post {postId} can rate its host.");

    public static Error AlreadyRated(PostId postId) =>
        Error.Conflict("ratings.rating.already_rated", $"The host of post {postId} has already been rated by this user.");

    public static readonly Error FeedbackEmpty =
        Error.Validation("ratings.feedback.empty", "The feedback cannot be blank.");

    public static readonly Error FeedbackTooLong =
        Error.Validation("ratings.feedback.too_long", $"The feedback can have at most {RatingFeedback.MaxLength} characters.");
}
