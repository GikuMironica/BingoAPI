using System.Text.Json;
using Hopaut.BuildingBlocks.Application;
using Hopaut.IntegrationEvents;

namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Publishes integration events by writing them to the outbox table.
/// The OutboxPublisher BackgroundService will later pick them up and dispatch.
/// </summary>
public sealed class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly OutboxDbContext _outboxDbContext;

    public OutboxIntegrationEventPublisher(OutboxDbContext outboxDbContext)
    {
        _outboxDbContext = outboxDbContext;
    }

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Type = integrationEvent.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
            OccurredOn = integrationEvent.OccurredOn
        };

        await _outboxDbContext.OutboxMessages.AddAsync(message, cancellationToken);
        await _outboxDbContext.SaveChangesAsync(cancellationToken);
    }
}
