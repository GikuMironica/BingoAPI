using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Attendance.Domain.Events;
using Mediator;

namespace Hopaut.Modules.Attendance.Infrastructure.DomainEventHandlers;

// Public: the Mediator source generator in the Host registers them (F-057). They run before the Attendance unit of work saves.

public sealed class AttendanceRequestedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AttendanceRequestedDomainEvent>
{
    public ValueTask Handle(AttendanceRequestedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AttendanceRequestedIntegrationEvent(
                notification.OccurredOnUtc, notification.ParticipationId, notification.PostId, notification.UserId, notification.AwaitsApproval),
            cancellationToken));
}

public sealed class AttendanceAcceptedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AttendanceAcceptedDomainEvent>
{
    public ValueTask Handle(AttendanceAcceptedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AttendanceAcceptedIntegrationEvent(notification.OccurredOnUtc, notification.ParticipationId, notification.PostId, notification.UserId),
            cancellationToken));
}

public sealed class AttendanceRejectedDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AttendanceRejectedDomainEvent>
{
    public ValueTask Handle(AttendanceRejectedDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AttendanceRejectedIntegrationEvent(notification.OccurredOnUtc, notification.ParticipationId, notification.PostId, notification.UserId),
            cancellationToken));
}

public sealed class AttendanceCancelledDomainEventHandler(IIntegrationEventPublisher publisher) : INotificationHandler<AttendanceCancelledDomainEvent>
{
    public ValueTask Handle(AttendanceCancelledDomainEvent notification, CancellationToken cancellationToken) =>
        new(publisher.PublishAsync(
            new AttendanceCancelledIntegrationEvent(notification.OccurredOnUtc, notification.ParticipationId, notification.PostId, notification.UserId),
            cancellationToken));
}
