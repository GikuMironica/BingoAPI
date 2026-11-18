namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Payload { get; init; }
    public required DateTime OccurredOn { get; init; }
    public DateTime? ProcessedOn { get; set; }
}
