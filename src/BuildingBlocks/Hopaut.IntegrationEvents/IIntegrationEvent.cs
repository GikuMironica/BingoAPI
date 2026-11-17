namespace Hopaut.IntegrationEvents;

/// <summary>
/// Marker interface for integration events that cross module boundaries.
/// These events are published through the outbox and consumed by other modules.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }
}

/// <summary>
/// Base record for integration events. The occurrence time comes from the domain event that caused it; the identifier is a
/// version-7 Guid and stays init-settable, so a deserialized event keeps the identifier consumers deduplicate on.
/// </summary>
public abstract record IntegrationEventBase : IIntegrationEvent
{
    protected IntegrationEventBase(DateTimeOffset occurredOnUtc)
    {
        OccurredOnUtc = occurredOnUtc.ToUniversalTime();
        EventId = Guid.CreateVersion7(OccurredOnUtc);
    }

    public Guid EventId { get; init; }

    public DateTimeOffset OccurredOnUtc { get; }
}
