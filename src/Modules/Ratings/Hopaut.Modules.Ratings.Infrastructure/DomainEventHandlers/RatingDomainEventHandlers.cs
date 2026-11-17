using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Ratings.Application.IntegrationEvents;
using Hopaut.Modules.Ratings.Domain.Events;
using Mediator;

namespace Hopaut.Modules.Ratings.Infrastructure.DomainEventHandlers;

// Public: the Mediator source generator in the Host registers them (F-057). They run before the Ratings unit of work saves.

public sealed class RatingCreatedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<RatingCreatedDomainEvent>
{
    public ValueTask Handle(RatingCreatedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new RatingCreatedIntegrationEvent(notification.OccurredOnUtc, notification.RatingId, notification.UserId, notification.RaterId, notification.PostId),
            cancellationToken));
}

public sealed class RatingDeletedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<RatingDeletedDomainEvent>
{
    public ValueTask Handle(RatingDeletedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new RatingDeletedIntegrationEvent(notification.OccurredOnUtc, notification.RatingId, notification.UserId),
            cancellationToken));
}
