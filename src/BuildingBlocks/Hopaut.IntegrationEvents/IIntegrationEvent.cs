namespace Hopaut.IntegrationEvents;

/// <summary>
/// Marker interface for integration events that cross module boundaries.
/// These events are published through the outbox and consumed by other modules.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

public abstract record IntegrationEventBase : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
