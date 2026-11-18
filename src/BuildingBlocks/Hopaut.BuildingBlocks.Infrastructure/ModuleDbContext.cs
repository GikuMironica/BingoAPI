using Hopaut.BuildingBlocks.Application;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace Hopaut.BuildingBlocks.Infrastructure;

/// <summary>
/// Base DbContext of a module and its unit of work. Saving first dispatches the domain events raised by tracked aggregates,
/// round after round until handlers raise no more, and only then writes everything in one transaction, so the handlers'
/// changes commit or roll back together with the aggregate (decision D-004). Each event is published by its runtime type.
/// Optimistic concurrency is per aggregate: a changed child marks its root as modified, so the root's concurrency token
/// (<c>xmin</c>) is checked and bumped; a stale token or a unique constraint taken concurrently surfaces as
/// <see cref="ConcurrencyConflictException"/>.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options, IPublisher publisher) : DbContext(options), IUnitOfWork
{
    private const int MaxDispatchRounds = 10;

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        TouchRootsOfChangedChildren();

        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException($"{GetType().Name}: an aggregate was changed concurrently.", exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConcurrencyConflictException($"{GetType().Name}: a unique value was taken concurrently.", exception);
        }
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

    /// <summary>
    /// An added, changed or removed child (an entity that is not an aggregate root) marks the tracked root it depends on
    /// as modified when that root has a concurrency token, so the save checks and bumps the token even though no column of
    /// the root changed.
    /// </summary>
    private void TouchRootsOfChangedChildren()
    {
        var changedChildren = ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && entry.Entity is not IHasDomainEvents)
            .ToList();

        foreach (var child in changedChildren)
        {
            foreach (var foreignKey in child.Metadata.GetForeignKeys())
            {
                var rootType = foreignKey.PrincipalEntityType;
                if (!typeof(IHasDomainEvents).IsAssignableFrom(rootType.ClrType) || !rootType.GetProperties().Any(p => p.IsConcurrencyToken))
                    continue;

                var rootKey = foreignKey.Properties
                    .Select(property => child.State == EntityState.Deleted ? child.OriginalValues[property] : child.CurrentValues[property])
                    .ToArray();
                var root = ChangeTracker.Entries().FirstOrDefault(entry =>
                    entry.Metadata == rootType
                    && foreignKey.PrincipalKey.Properties.Select(property => entry.CurrentValues[property]).SequenceEqual(rootKey));

                if (root is { State: EntityState.Unchanged })
                    MarkModified(root);
            }
        }
    }

    private static void MarkModified(EntityEntry root)
    {
        var property = root.Properties.FirstOrDefault(property =>
            !property.Metadata.IsKey() && !property.Metadata.IsConcurrencyToken && property.Metadata.ValueGenerated == ValueGenerated.Never);
        if (property is not null)
            property.IsModified = true;
        else
            root.State = EntityState.Modified;
    }
}
