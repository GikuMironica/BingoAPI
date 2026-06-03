using Hopaut.IntegrationEvents;

namespace Hopaut.BuildingBlocks.Application;

/// <summary>
/// Publishes integration events to the outbox for cross-module communication.
/// Domain event handlers use this to translate domain events into integration events.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
