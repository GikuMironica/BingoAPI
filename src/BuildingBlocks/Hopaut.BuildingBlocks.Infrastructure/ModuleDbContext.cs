using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.BuildingBlocks.Infrastructure;

/// <summary>
/// Base DbContext of a module and its unit of work. Saving first dispatches the domain events raised by tracked aggregates,
/// round after round until handlers raise no more, and only then writes everything in one transaction, so the handlers'
/// changes commit or roll back together with the aggregate (decision D-004). Each event is published by its runtime type.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options, IPublisher publisher) : DbContext(options), IUnitOfWork
{
    private const int MaxDispatchRounds = 10;

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>A synchronous save cannot dispatch domain events, so it would silently drop them.</summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new NotSupportedException($"{GetType().Name} saves only through SaveChangesAsync, which dispatches domain events first.");

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        for (var round = 0; round < MaxDispatchRounds; round++)
        {
            var aggregates = ChangeTracker.Entries<IHasDomainEvents>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count > 0)
                .ToList();

            if (aggregates.Count == 0)
            {
                return;
            }

            var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await publisher.Publish((object)domainEvent, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Domain events of {GetType().Name} did not settle after {MaxDispatchRounds} dispatch rounds.");
    }
}
