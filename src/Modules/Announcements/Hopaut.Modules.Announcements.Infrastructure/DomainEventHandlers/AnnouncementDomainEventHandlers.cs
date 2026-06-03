using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Announcements.Application.IntegrationEvents;
using Hopaut.Modules.Announcements.Domain.Events;
using MediatR;

namespace Hopaut.Modules.Announcements.Infrastructure.DomainEventHandlers;

internal sealed class AnnouncementCreatedDomainEventHandler : INotificationHandler<AnnouncementCreatedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public AnnouncementCreatedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(AnnouncementCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new AnnouncementCreatedIntegrationEvent(notification.AnnouncementId, notification.PostId),
            cancellationToken);
    }
}
