namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Writes integration events to the outbox table within the current transaction.
/// Each module's DbContext should implement this or delegate to a shared helper.
/// </summary>
public interface IOutboxWriter
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
