using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain;

/// <summary>Expected failures of the Rating aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class RatingErrors
{
    public static Error NotFound(RatingId ratingId) =>
        Error.NotFound("ratings.rating.not_found", $"Rating {ratingId} was not found.");

    public static Error NotRater(RatingId ratingId) =>
        Error.Forbidden("ratings.rating.not_rater", $"Only the author of rating {ratingId} or an administrator can delete it.");
}
