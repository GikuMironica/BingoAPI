using Hopaut.IntegrationEvents;

namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; set; }
}
