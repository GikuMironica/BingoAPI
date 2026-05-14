namespace Hopaut.BuildingBlocks.Infrastructure.Inbox;

/// <summary>
/// Tracks processed integration events to ensure idempotent consumption.
/// </summary>
public sealed class InboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = default!;
    public DateTime ProcessedOn { get; init; } = DateTime.UtcNow;
}
