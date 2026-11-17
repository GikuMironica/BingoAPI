using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Posts.Application.IntegrationEvents;
using Hopaut.Modules.Posts.Domain.Events;
using Mediator;

namespace Hopaut.Modules.Posts.Infrastructure.DomainEventHandlers;

// Public: the Mediator source generator in the Host registers them (F-057). They run before the Posts unit of work saves.

public sealed class PostCreatedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<PostCreatedDomainEvent>
{
    public ValueTask Handle(PostCreatedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new PostCreatedIntegrationEvent(notification.OccurredOnUtc, notification.PostId, notification.UserId),
            cancellationToken));
}

public sealed class PostDeletedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<PostDeletedDomainEvent>
{
    public ValueTask Handle(PostDeletedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new PostDeletedIntegrationEvent(notification.OccurredOnUtc, notification.PostId),
            cancellationToken));
}

public sealed class PictureStateChangedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<PictureStateChangedDomainEvent>
{
    public ValueTask Handle(PictureStateChangedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new PictureStateChangedIntegrationEvent(notification.OccurredOnUtc, notification.PictureId, notification.PostId, notification.NewState.ToString()),
            cancellationToken));
}
