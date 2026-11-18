# Messaging skeletons (Hopaut)

Reference shapes for [SKILL.md](../SKILL.md). Verify Mediator and MassTransit APIs against the versions pinned in `Directory.Packages.props` before copying; adapt names to the module.

## Contents
1. Mediator registration (Host)
2. Result contract used by behaviors
3. Validation and unit-of-work behaviors
4. ModuleDbContext: dispatch before save
5. Outbox message, event-type registry, writer
6. Domain event → integration event translator
7. Outbox relay
8. In-process bus and idempotent consumer
9. MassTransit adapter

## 1. Mediator registration (Host)

```csharp
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies =
    [
        typeof(Hopaut.Modules.Posts.Application.AssemblyMarker),
        typeof(Hopaut.Modules.Posts.Infrastructure.AssemblyMarker),
        typeof(Hopaut.Modules.Attendance.Application.AssemblyMarker),
        typeof(Hopaut.Modules.Attendance.Infrastructure.AssemblyMarker),
        // every Application / Infrastructure assembly that contains handlers
    ];
    options.PipelineBehaviors =
    [
        typeof(LoggingBehavior<,>),
        typeof(ValidationBehavior<,>),
        typeof(CachingBehavior<,>),
        typeof(PostsUnitOfWorkBehavior<,>),
        typeof(AttendanceUnitOfWorkBehavior<,>),
        // one unit-of-work behavior per module with persistence
    ];
});
```

## 2. Result contract used by behaviors

`Result` and `Result<T>` implement `IResultBase` (success check) and `IFailureFactory<TSelf>` (typed failure construction); see section 1 of the clean-architecture-ddd [patterns](../../clean-architecture-ddd/references/patterns.md). Behaviors constrain `TResponse` to these interfaces instead of using reflection.

## 3. Validation and unit-of-work behaviors

```csharp
// BuildingBlocks.Application
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : IFailureFactory<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
            return await next(message, cancellationToken);

        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        return TResponse.Failure(new ValidationError(errors));
    }
}

public abstract class UnitOfWorkBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
    where TResponse : IResultBase
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);
        if (response.IsSuccess)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}

// Modules.Posts.Application
public interface IPostsCommand : IBaseCommand;
public interface IPostsUnitOfWork : IUnitOfWork;

public sealed class PostsUnitOfWorkBehavior<TMessage, TResponse>(IPostsUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IPostsCommand
    where TResponse : IResultBase;

// Modules.Posts.Infrastructure installer
services.AddScoped<IPostsUnitOfWork>(sp => sp.GetRequiredService<PostsModuleDbContext>());
```

## 4. ModuleDbContext: dispatch before save

```csharp
public abstract class ModuleDbContext(DbContextOptions options, IPublisher publisher) : DbContext(options), IUnitOfWork
{
    private const int MaxDispatchRounds = 10;

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        for (var round = 0; round < MaxDispatchRounds; round++)
        {
            var pending = ChangeTracker.Entries<IHasDomainEvents>()
                .Select(entry => entry.Entity)
                .Where(entity => entity.DomainEvents.Count > 0)
                .ToList();

            if (pending.Count == 0)
                return;

            var events = pending.SelectMany(entity => entity.DomainEvents).ToList();
            pending.ForEach(entity => entity.ClearDomainEvents());

            foreach (var domainEvent in events)
                await publisher.Publish((object)domainEvent, cancellationToken); // runtime-type dispatch
        }

        throw new InvalidOperationException($"Domain events did not settle after {MaxDispatchRounds} rounds.");
    }
}
```

## 5. Outbox message, event-type registry, writer

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Payload { get; init; }
    public DateTimeOffset OccurredOnUtc { get; init; }
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset? NextAttemptOnUtc { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset? DeadLetteredOnUtc { get; set; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IntegrationEventNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

// Built once at startup from every module Contracts assembly; fails fast on missing or duplicate names.
public sealed class IntegrationEventTypeRegistry
{
    private readonly FrozenDictionary<string, Type> _typesByName;
    private readonly FrozenDictionary<Type, string> _namesByType;

    public IntegrationEventTypeRegistry(IEnumerable<Assembly> contractsAssemblies)
    {
        var pairs = contractsAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IIntegrationEvent).IsAssignableFrom(t))
            .Select(t => (Name: t.GetCustomAttribute<IntegrationEventNameAttribute>()?.Name
                              ?? throw new InvalidOperationException($"{t.Name} has no [IntegrationEventName]."), Type: t))
            .ToList();

        _typesByName = pairs.ToFrozenDictionary(p => p.Name, p => p.Type); // throws on duplicate names
        _namesByType = pairs.ToFrozenDictionary(p => p.Type, p => p.Name);
    }

    public string NameOf(Type type) => _namesByType[type];
    public Type TypeOf(string name) => _typesByName[name];
}

public sealed class OutboxWriter<TDbContext>(TDbContext db, IntegrationEventTypeRegistry registry)
    where TDbContext : ModuleDbContext
{
    public void Add(IIntegrationEvent integrationEvent) =>
        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Type = registry.NameOf(integrationEvent.GetType()),
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), JsonSerializerOptions.Web),
            OccurredOnUtc = integrationEvent.OccurredOnUtc,
        });
}
```

## 6. Domain event → integration event translator

```csharp
// Modules.Posts.Infrastructure
public sealed class PostCreatedDomainEventHandler(OutboxWriter<PostsModuleDbContext> outbox)
    : INotificationHandler<PostCreatedDomainEvent>
{
    public ValueTask Handle(PostCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Add(new PostCreatedIntegrationEvent(
            Guid.CreateVersion7(), domainEvent.OccurredOnUtc, domainEvent.PostId.Value, domainEvent.HostId.Value));
        return ValueTask.CompletedTask;
    }
}

// Modules.Posts.Contracts
[IntegrationEventName("posts.post-created.v1")]
public sealed record PostCreatedIntegrationEvent(Guid EventId, DateTimeOffset OccurredOnUtc, int PostId, string HostUserId)
    : IIntegrationEvent;
```

## 7. Outbox relay

```csharp
public sealed class OutboxRelay<TDbContext>(
    IServiceScopeFactory scopes,
    TimeProvider clock,
    IOptions<OutboxOptions> options,
    ILogger<OutboxRelay<TDbContext>> logger) : BackgroundService
    where TDbContext : ModuleDbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollingInterval, clock);
        do
        {
            try
            {
                await RelayBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Outbox relay for {DbContext} failed", typeof(TDbContext).Name);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RelayBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>();
        var registry = scope.ServiceProvider.GetRequiredService<IntegrationEventTypeRegistry>();
        var now = clock.GetUtcNow();

        // Single instance: LINQ. Several instances: FOR UPDATE SKIP LOCKED inside a transaction.
        var batch = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.DeadLetteredOnUtc == null
                        && (m.NextAttemptOnUtc == null || m.NextAttemptOnUtc <= now))
            .OrderBy(m => m.OccurredOnUtc)
            .Take(options.Value.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in batch)
        {
            try
            {
                var integrationEvent = (IIntegrationEvent)JsonSerializer.Deserialize(
                    message.Payload, registry.TypeOf(message.Type), JsonSerializerOptions.Web)!;
                await bus.PublishAsync(integrationEvent, cancellationToken);
                message.ProcessedOnUtc = clock.GetUtcNow();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.Attempts++;
                message.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;

                if (message.Attempts >= options.Value.MaxAttempts)
                {
                    message.DeadLetteredOnUtc = clock.GetUtcNow();
                    logger.LogError(ex, "Outbox message {MessageId} ({Type}) dead-lettered", message.Id, message.Type);
                }
                else
                {
                    message.NextAttemptOnUtc = clock.GetUtcNow() + TimeSpan.FromSeconds(Math.Pow(2, message.Attempts));
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
```

## 8. In-process bus and idempotent consumer

```csharp
public interface IIntegrationEventBus
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}

public sealed class InProcessIntegrationEventBus(IServiceScopeFactory scopes) : IIntegrationEventBus
{
    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope(); // consumers get fresh module DbContexts
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        await publisher.Publish((object)integrationEvent, cancellationToken);
    }
}

public sealed class InboxMessage
{
    public Guid EventId { get; init; }
    public required string Consumer { get; init; } // unique together with EventId
    public DateTimeOffset ProcessedOnUtc { get; init; }
}

public sealed class Inbox<TDbContext>(TDbContext db, TimeProvider clock) where TDbContext : ModuleDbContext
{
    // Adds the inbox row without saving: the consumer's command saves it with the state change.
    public async Task<bool> TryAcceptAsync(Guid eventId, string consumer, CancellationToken cancellationToken)
    {
        if (await db.InboxMessages.AnyAsync(m => m.EventId == eventId && m.Consumer == consumer, cancellationToken))
            return false;

        db.InboxMessages.Add(new InboxMessage { EventId = eventId, Consumer = consumer, ProcessedOnUtc = clock.GetUtcNow() });
        return true;
    }
}

// Modules.Attendance.Infrastructure
public sealed class PostDeletedIntegrationEventHandler(
    Inbox<AttendanceModuleDbContext> inbox,
    ISender sender,
    ILogger<PostDeletedIntegrationEventHandler> logger) : INotificationHandler<PostDeletedIntegrationEvent>
{
    public async ValueTask Handle(PostDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (!await inbox.TryAcceptAsync(integrationEvent.EventId, nameof(PostDeletedIntegrationEventHandler), cancellationToken))
            return;

        var result = await sender.Send(new CancelParticipationsForPostCommand(new PostId(integrationEvent.PostId)), cancellationToken);
        if (result.IsFailure)
            logger.LogWarning("Ignoring {EventId}: {ErrorCode}", integrationEvent.EventId, result.Error.Code);
    }
}
```

When a permanent business failure is acknowledged, the command saved nothing, so the inbox row is not persisted either; a redelivery reaches the same no-op outcome. Throw for transient failures so the relay retries.

## 9. MassTransit adapter

Only after D-MSG-2 is satisfied; confirm every API below against the pinned MassTransit version.

```csharp
public sealed class MassTransitIntegrationEventBus(IPublishEndpoint publishEndpoint) : IIntegrationEventBus
{
    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), cancellationToken);
}

// Forwards broker deliveries to the same Mediator notification handlers used in-process.
public sealed class IntegrationEventConsumer<TEvent>(IPublisher publisher) : IConsumer<TEvent>
    where TEvent : class, IIntegrationEvent
{
    public async Task Consume(ConsumeContext<TEvent> context) =>
        await publisher.Publish(context.Message, context.CancellationToken);
}
```

Register one closed `IntegrationEventConsumer<TEvent>` per event type from the registry, configure retry and redelivery per receive endpoint, and keep transport credentials in configuration, never in the repository.
