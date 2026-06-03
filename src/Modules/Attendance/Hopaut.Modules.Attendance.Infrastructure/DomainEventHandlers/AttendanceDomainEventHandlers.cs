using Hopaut.BuildingBlocks.Application;
using Hopaut.Modules.Attendance.Application.IntegrationEvents;
using Hopaut.Modules.Attendance.Domain.Events;
using MediatR;

namespace Hopaut.Modules.Attendance.Infrastructure.DomainEventHandlers;

internal sealed class AttendanceRequestedDomainEventHandler : INotificationHandler<AttendanceRequestedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public AttendanceRequestedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(AttendanceRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new AttendanceRequestedIntegrationEvent(notification.ParticipationId, notification.PostId, notification.UserId),
            cancellationToken);
    }
}

internal sealed class AttendanceAcceptedDomainEventHandler : INotificationHandler<AttendanceAcceptedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public AttendanceAcceptedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(AttendanceAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new AttendanceAcceptedIntegrationEvent(notification.ParticipationId, notification.PostId, notification.UserId),
            cancellationToken);
    }
}

internal sealed class AttendanceRejectedDomainEventHandler : INotificationHandler<AttendanceRejectedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public AttendanceRejectedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(AttendanceRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new AttendanceRejectedIntegrationEvent(notification.ParticipationId, notification.PostId, notification.UserId),
            cancellationToken);
    }
}
