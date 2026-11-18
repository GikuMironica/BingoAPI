using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Announcements.Contracts;
using Hopaut.Modules.Announcements.Domain.Events;
using Mediator;

namespace Hopaut.Modules.Announcements.Infrastructure.DomainEventHandlers;

// Public: the Mediator source generator in the Host registers them (F-057). They run before the Announcements unit of work saves.

public sealed class AnnouncementCreatedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AnnouncementCreatedDomainEvent>
{
    public ValueTask Handle(AnnouncementCreatedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AnnouncementCreatedIntegrationEvent(notification.OccurredOnUtc, notification.AnnouncementId, notification.PostId),
            cancellationToken));
}

public sealed class AnnouncementEditedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AnnouncementEditedDomainEvent>
{
    public ValueTask Handle(AnnouncementEditedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AnnouncementEditedIntegrationEvent(notification.OccurredOnUtc, notification.AnnouncementId, notification.PostId),
            cancellationToken));
}

public sealed class AnnouncementDeletedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AnnouncementDeletedDomainEvent>
{
    public ValueTask Handle(AnnouncementDeletedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AnnouncementDeletedIntegrationEvent(notification.OccurredOnUtc, notification.AnnouncementId, notification.PostId),
            cancellationToken));
}
