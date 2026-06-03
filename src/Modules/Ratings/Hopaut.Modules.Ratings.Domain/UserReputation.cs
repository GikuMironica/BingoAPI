using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain;

/// <summary>
/// Denormalized projection maintained by the Ratings module.
/// Updated on every rating create/delete.
/// </summary>
public sealed class UserReputation
{
    public UserId UserId { get; set; }
    public int TotalRatings { get; set; }
    public int SumRatings { get; set; }
    public double AverageRating { get; set; }
}
