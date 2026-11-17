using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Application.Queries;

/// <summary>A rating as the read endpoints show it (today's wire shape, plan Step 10.7 changes it).</summary>
public sealed record RatingDto(int Id, int Rate, string RaterId, int PostId, string? Feedback, DateTimeOffset CreatedAt);

/// <summary>Rating reads served by no-tracking projections (never loading aggregates).</summary>
public interface IRatingReadModel
{
    Task<RatingDto?> GetByIdAsync(RatingId ratingId, CancellationToken ct);

    /// <summary>The ratings the user received, newest first.</summary>
    Task<IReadOnlyList<RatingDto>> GetReceivedAsync(UserId userId, CancellationToken ct);
}
