using Mediator;

namespace Hopaut.SharedKernel;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }
}

/// <summary>
/// Base record for domain events. Aggregates pass the time they received (<c>now</c>), so no event reads a clock; the
/// identifier is a version-7 Guid whose timestamp is the occurrence time.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    protected DomainEventBase(DateTimeOffset occurredOnUtc)
    {
        OccurredOnUtc = occurredOnUtc.ToUniversalTime();
        EventId = Guid.CreateVersion7(OccurredOnUtc);
    }

    public Guid EventId { get; }

    public DateTimeOffset OccurredOnUtc { get; }
}
