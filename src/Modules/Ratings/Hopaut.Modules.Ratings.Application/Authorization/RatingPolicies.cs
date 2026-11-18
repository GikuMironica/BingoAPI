namespace Hopaut.Modules.Ratings.Application.Authorization;

/// <summary>Resource-based policies of the Ratings module (plan Step 6.9), registered by <c>AddRatingsModule</c>.</summary>
public static class RatingPolicies
{
    /// <summary>The caller wrote the rating, or is an administrator (plan Step 8.4; the legacy API let only administrators delete).</summary>
    public const string MustOwnRating = "ratings.rating.owner";
}
