using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Posts.Application.IntegrationEvents;
using Hopaut.Modules.Posts.Domain.Events;
using MediatR;

namespace Hopaut.Modules.Posts.Infrastructure.DomainEventHandlers;

internal sealed class PostCreatedDomainEventHandler : INotificationHandler<PostCreatedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public PostCreatedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(PostCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new PostCreatedIntegrationEvent(notification.PostId, notification.UserId),
            cancellationToken);
    }
}

internal sealed class PostDeletedDomainEventHandler : INotificationHandler<PostDeletedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public PostDeletedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(PostDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new PostDeletedIntegrationEvent(notification.PostId),
            cancellationToken);
    }
}

internal sealed class PictureStateChangedDomainEventHandler : INotificationHandler<PictureStateChangedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public PictureStateChangedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(PictureStateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new PictureStateChangedIntegrationEvent(notification.PictureId, notification.PostId, notification.NewState.ToString()),
            cancellationToken);
    }
}
