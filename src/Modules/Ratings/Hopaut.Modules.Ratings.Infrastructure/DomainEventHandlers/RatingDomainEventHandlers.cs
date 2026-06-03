using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Ratings.Application.IntegrationEvents;
using Hopaut.Modules.Ratings.Domain.Events;
using MediatR;

namespace Hopaut.Modules.Ratings.Infrastructure.DomainEventHandlers;

internal sealed class RatingCreatedDomainEventHandler : INotificationHandler<RatingCreatedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public RatingCreatedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(RatingCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new RatingCreatedIntegrationEvent(notification.RatingId, notification.UserId, notification.RaterId, notification.PostId),
            cancellationToken);
    }
}

internal sealed class RatingDeletedDomainEventHandler : INotificationHandler<RatingDeletedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public RatingDeletedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(RatingDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new RatingDeletedIntegrationEvent(notification.RatingId, notification.UserId),
            cancellationToken);
    }
}
