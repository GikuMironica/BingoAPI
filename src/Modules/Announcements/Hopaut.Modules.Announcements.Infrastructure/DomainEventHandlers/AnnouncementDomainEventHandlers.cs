using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Announcements.Contracts;
using Hopaut.Modules.Announcements.Domain.Events;
using Mediator;

namespace Hopaut.Modules.Announcements.Infrastructure.DomainEventHandlers;

// Public: the Mediator source generator in the Host registers it (F-057). It runs before the Announcements unit of work saves.
public sealed class AnnouncementCreatedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AnnouncementCreatedDomainEvent>
{
    public ValueTask Handle(AnnouncementCreatedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AnnouncementCreatedIntegrationEvent(notification.OccurredOnUtc, notification.AnnouncementId, notification.PostId),
            cancellationToken));
}
