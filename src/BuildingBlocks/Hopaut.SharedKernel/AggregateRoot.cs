namespace Hopaut.SharedKernel;

/// <summary>Consistency boundary. Only the aggregate itself raises domain events, through <see cref="Raise"/>.</summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
