namespace Hopaut.SharedKernel;

/// <summary>Lets persistence collect and clear the domain events an aggregate raised.</summary>
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
