using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Infrastructure.Reputation;

/// <summary>
/// Denormalised reputation of a rated user — how many ratings they received, their sum and average — kept by
/// <see cref="ReputationProjection"/> in the same save as each rating change and read through <c>IUserReputationQueries</c>.
/// A read model, not an aggregate (decision D-031). <see cref="Version"/> (PostgreSQL <c>xmin</c>) makes two ratings of the
/// same user saved at the same time a concurrency conflict instead of a lost update (F-017).
/// </summary>
public sealed class UserReputation
{
    private UserReputation() { } // EF Core

    public UserId UserId { get; private set; }

    public int TotalRatings { get; private set; }

    public int SumRatings { get; private set; }

    public double AverageRating { get; private set; }

    public uint Version { get; private set; }

    /// <summary>No rating is counted any more; the projection drops the row (users without ratings have no reputation).</summary>
    public bool IsEmpty => TotalRatings <= 0;

    public static UserReputation Start(UserId userId) => new() { UserId = userId };

    public void Count(int rate) => Recalculate(TotalRatings + 1, SumRatings + rate);

    public void Discount(int rate) => Recalculate(TotalRatings - 1, SumRatings - rate);

    private void Recalculate(int total, int sum)
    {
        TotalRatings = total;
        SumRatings = sum;
        AverageRating = total > 0 ? (double)sum / total : 0;
    }
}
