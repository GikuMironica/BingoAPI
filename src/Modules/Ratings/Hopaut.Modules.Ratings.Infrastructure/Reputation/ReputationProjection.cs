using Hopaut.Modules.Ratings.Domain.Events;
using Mediator;

namespace Hopaut.Modules.Ratings.Infrastructure.Reputation;

// Public: the Mediator source generator in the Host registers them (F-057). They run before the Ratings unit of work saves,
// so the projection commits with the rating change (D-004, D-031).

/// <summary>Counts a new rating in the rated user's reputation.</summary>
public sealed class ReputationProjection(RatingsModuleDbContext db)
    : INotificationHandler<RatingCreatedDomainEvent>, INotificationHandler<RatingDeletedDomainEvent>
{
    public async ValueTask Handle(RatingCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var reputation = await db.UserReputations.FindAsync([notification.UserId], cancellationToken);
        if (reputation is null)
        {
            reputation = UserReputation.Start(notification.UserId);
            db.UserReputations.Add(reputation);
        }

        reputation.Count(notification.Rate.Value);
    }

    public async ValueTask Handle(RatingDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var reputation = await db.UserReputations.FindAsync([notification.UserId], cancellationToken);
        if (reputation is null)
            return;

        reputation.Discount(notification.Rate.Value);
        if (reputation.IsEmpty)
            db.UserReputations.Remove(reputation);
    }
}
