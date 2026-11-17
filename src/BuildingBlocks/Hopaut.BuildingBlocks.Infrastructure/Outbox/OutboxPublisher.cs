using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hopaut.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Background service that polls the outbox table. Not registered: it marks messages processed without publishing them
/// (finding F-005); plan Step 9.2 replaces it with a relay that publishes through IIntegrationEventBus.
/// </summary>
public sealed class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public OutboxPublisher(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisher> logger, TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            _logger.LogInformation("Publishing outbox message {MessageId} of type {Type}", message.Id, message.Type);
            // Plan Step 9.2: deserialize through the event-type registry and publish via IIntegrationEventBus before marking.
            message.ProcessedOn = _timeProvider.GetUtcNow().UtcDateTime;
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Processed {Count} outbox message(s)", messages.Count);
    }
}
