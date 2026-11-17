namespace Hopaut.BuildingBlocks.Infrastructure.Inbox;

/// <summary>
/// Tracks processed integration events to ensure idempotent consumption.
/// </summary>
public sealed class InboxMessage
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required DateTime ProcessedOn { get; init; }
}
