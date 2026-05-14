using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Ratings.Application.Queries.GetReputationsForUsers;
using MediatR;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>
/// Bridges the Posts module to the Ratings module via MediatR,
/// keeping the module boundary at the Application layer clean.
/// </summary>
public sealed class MediatRUserReputationProvider : IUserReputationProvider
{
    private readonly ISender _sender;

    public MediatRUserReputationProvider(ISender sender) => _sender = sender;

    public async Task<Dictionary<string, UserReputationInfo>> GetBatchAsync(IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        if (ids.Count == 0) return new();

        var result = await _sender.Send(new GetReputationsForUsersQuery(ids), ct);

        return result.ToDictionary(
            kvp => kvp.Key,
            kvp => new UserReputationInfo(kvp.Value.TotalRatings, kvp.Value.AverageRating));
    }
}
