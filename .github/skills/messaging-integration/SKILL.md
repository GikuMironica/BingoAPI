---
name: messaging-integration
description: "Messaging and module-decoupling standards for the Hopaut modular monolith (BingoAPI). Use when: dispatching commands or queries, writing pipeline behaviors, raising or handling domain events, publishing or consuming integration events between modules, migrating MediatR to Mediator (source generator), adopting MassTransit, implementing or fixing the transactional outbox and inbox, idempotent consumers, retries, Hangfire jobs, or cross-module queries."
user-invocable: false
---

# Messaging and integration

Leading word: **boundaries**. Modules share only Contracts, and everything that crosses a boundary is a message persisted before it is sent.

Code skeletons: [outbox-and-bus.md](./references/outbox-and-bus.md).

## 1. Message kinds

| Kind | Scope | Mechanism | Delivery | Defined in |
|------|-------|-----------|----------|------------|
| Command | one module, sync | Mediator `ICommand<TResult>` + module marker | exactly one handler, inside the module's unit of work | `Modules.<X>.Application` |
| Query | one module, sync | Mediator `IQuery<TResult>` | exactly one handler, no writes, may be cached | `Modules.<X>.Application` |
| Domain event | one module, same transaction | `IDomainEvent : INotification` | every handler, in-process, before the save | `Modules.<X>.Domain` |
| Integration event | cross-module, async | module outbox → relay → `IIntegrationEventBus` | at least once; consumers idempotent | `Modules.<X>.Contracts` |
| Cross-module read | cross-module, sync | query interface in the owner's Contracts | direct call, read-only, batched | owner's `Contracts` (interface) + `Infrastructure` (implementation) |

Nothing else crosses a module boundary. Calling another module's handler, repository or DbContext is a defect.

## 2. Decisions (confirmed by the user on 2026-11-17; record any later change in TODO.md *Decisions*)

- **D-MSG-1 In-process mediator: `Mediator` 3.x** (martinothamar, MIT, source-generated) replaces MediatR 12.x, whose successors (13+) are commercially licensed. Gains: build-time diagnostics for missing or duplicate handlers, `ValueTask`, no reflection.
- **D-MSG-2 Integration transport behind `IIntegrationEventBus`.** `InProcessIntegrationEventBus` while the monolith runs as one process. `MassTransitIntegrationEventBus` when workers are extracted (plan Step 6.12) or the user asks for it, and only on a license-compliant version: 8.x (Apache-2.0) pinned below 9, or 9.x with a user-supplied license key read from configuration. Before adopting, fetch MassTransit's current license and support status and record it.
- **D-MSG-3 Hand-rolled outbox per module.** Each module's DbContext owns `outbox_messages` and `inbox_messages` in its schema, so the outbox row commits atomically with the aggregate whatever the transport.

## 3. Mediator rules

- `Mediator.Abstractions` in SharedKernel and Application/Infrastructure projects; `Mediator.SourceGenerator` **only** in `Hopaut.Api.Host` (installing it in several projects of one app breaks generation).
- Configure `AddMediator` inline with literal `typeof(...)` expressions; the generator reads that syntax at compile time and ignores computed values.
- `ServiceLifetime.Scoped` (handlers depend on scoped DbContexts; the default singleton lifetime would capture them).
- List every assembly that contains handlers, including module Infrastructure assemblies with domain-event or integration-event handlers.
- Messages and handlers are `public`; the generated registrations in the Host must see them.
- Signatures: `ValueTask<TResult> Handle(TMessage message, CancellationToken cancellationToken)`; behaviors call `next(message, cancellationToken)`.
- Escalate the generator's missing-handler and duplicate-handler diagnostics to errors: take their IDs from the build output and add them to `WarningsAsErrors`.
- Pipeline order, outer to inner: `LoggingBehavior` → `ValidationBehavior` → `CachingBehavior` (queries with `ICachedQuery`) → module `UnitOfWorkBehavior` (commands with that module's marker) → handler. Behaviors select messages through generic constraints, never runtime type checks.
- Every command and query returns `Result` or `Result<T>`; otherwise the constrained behaviors silently skip it (enforced by an architecture test).

**MediatR → Mediator recipe**
1. Add both Mediator packages to `Directory.Packages.props`.
2. `IRequest<T>` → `ICommand<T>` (writes) or `IQuery<T>` (reads); `IRequestHandler` → `ICommandHandler` / `IQueryHandler`; `Task<T>` → `ValueTask<T>`.
3. Notification handlers return `ValueTask`. `ISender`, `IPublisher` and `IMediator` keep their names (namespace `Mediator`).
4. Port behaviors to `MessageHandlerDelegate<TMessage, TResponse>`.
5. Replace `AddMediatR(...)` with one `AddMediator(...)` in the Host; delete per-module MediatR registrations.
6. Remove MediatR from CPM and add an architecture test that no assembly references `MediatR`.
7. Gate: full build and all tests green.

## 4. Unit of work and domain events

- Each module defines a command marker (`IPostsCommand : IBaseCommand`) and a unit-of-work port (`IPostsUnitOfWork : IUnitOfWork`, implemented by its DbContext). Its `PostsUnitOfWorkBehavior` saves once, after a successful handler result; a failed result saves nothing.
- `ModuleDbContext.SaveChangesAsync` dispatches pending domain events **before** calling `base.SaveChangesAsync`, looping until none remain. Handlers' changes (same-module aggregates, outbox rows) join that single save, so everything commits atomically. Publish each event by its runtime type, and prove it with a test that a concrete-event handler runs.
- Pre-assigned IDs (clean-architecture-ddd §2) make events correct before the save; never dispatch after saving to obtain database-generated keys.
- Domain-event handlers touch only their own module. Anything else becomes an integration event written through the module's `OutboxWriter`.
- Only unit-of-work behaviors, the outbox relay and test fixtures call `SaveChangesAsync`.

## 5. Outbox and relay

- **Write side:** a domain-event handler in the module's Infrastructure maps the domain event to a Contracts integration event and calls `OutboxWriter<TModuleDbContext>.Add(...)`. There is no global `IIntegrationEventPublisher` registration, so no module can write into another module's outbox.
- **Message:** `Id` (= `EventId`, Guid v7), stable `Type` name (`posts.post-created.v1`, from the event-type registry; never `AssemblyQualifiedName`), JSON `Payload` (`JsonSerializerOptions.Web`), `OccurredOnUtc`, `ProcessedOnUtc`, `Attempts`, `NextAttemptOnUtc`, `LastError`, `DeadLetteredOnUtc`. All timestamps `DateTimeOffset`.
- **Relay:** one `OutboxRelay<TModuleDbContext>` per module; `PeriodicTimer` + stopping token; new scope per batch; select due, unprocessed, non-dead messages ordered by `OccurredOnUtc`; deserialize via the registry; publish through `IIntegrationEventBus`; set `ProcessedOnUtc` **only after a successful publish**. On failure increment `Attempts`, store a truncated `LastError`, schedule `NextAttemptOnUtc` with exponential backoff, and dead-letter after `MaxAttempts` with an Error log. Save once per batch.
- When the API runs more than one instance, claim rows with `FOR UPDATE SKIP LOCKED` (raw SQL using the schema-qualified table name from the EF model); with a single instance, LINQ is enough. Record which applies.
- Delivery is at-least-once and not globally ordered; consumers must tolerate duplicates and reordering.
- A catch-all `catch (Exception)` is legitimate only at process boundaries like this relay, and must record the failure.

## 6. Integration events and consumers

- Contracts: `public sealed record <Noun><PastVerb>IntegrationEvent(Guid EventId, DateTimeOffset OccurredOnUtc, …) : IIntegrationEvent`, decorated with a stable versioned name. Payloads use primitives and SharedKernel IDs only, the deliberate exception to the no-primitives rule. Evolve additively; a breaking change is a new `.v2` event while both are published.
- Consumers are `INotificationHandler<TEvent>` in the consuming module, independent of transport. They are idempotent: `inbox.TryAcceptAsync(eventId, consumerName)` adds an inbox row (unique on `EventId` + consumer) to the module DbContext, then the consumer sends a command of its own module, whose unit of work saves the inbox row and the state change together. A duplicate returns early.
- Transient failures throw so the relay retries. Permanent business failures (target gone, already in the desired state) are logged and acknowledged; never retried forever.
- Push, e-mail and S3 side effects live only in the Notifications and Media modules, triggered by integration events.

## 7. MassTransit adapter (only after D-MSG-2 is satisfied)

- MassTransit types appear only in BuildingBlocks.Infrastructure and the Host. Modules keep their `INotificationHandler<TEvent>` consumers; one generic `IntegrationEventConsumer<TEvent> : IConsumer<TEvent>` forwards to Mediator in a fresh scope.
- Transport: PostgreSQL SQL transport or in-memory inside the monolith; RabbitMQ only after worker extraction. Tests use the MassTransit test harness.
- Keep the module outbox (D-MSG-3) as the source of truth; the relay publishes through `MassTransitIntegrationEventBus`. Configure consumer retry and redelivery explicitly.

## 8. Cross-module reads

- The owner publishes a read-only interface in its Contracts (`IUserReputationQueries.GetAsync(IReadOnlyCollection<UserId> ids, CancellationToken)`) with Contracts DTOs, implemented in its Infrastructure and registered by its installer.
- Batch-shaped APIs only (no per-item calls inside loops); never return domain types.
- The current `Posts.Infrastructure → Ratings.Application` adapter is exactly what this replaces (plan Step 6.6).

## 9. Background jobs (Hangfire)

- Only for scheduled or recurring work and long-running processing such as the picture pipeline.
- Never enqueue from inside a command handler (the job could run before the commit or after a rollback); enqueue from an integration-event consumer.
- Job arguments are IDs; jobs are idempotent and resolve dependencies through DI.
- The dashboard is reachable only for the Admin role.

## 10. Detection list

| Problem | Where to look | Fix |
|---------|---------------|-----|
| Relay marks messages processed without publishing | `ProcessedOn` set in `OutboxPublisher` with a TODO instead of a publish | publish, then mark |
| Outbox written in a separate DbContext or transaction | `OutboxDbContext` + `SaveChangesAsync` inside the publisher | module outbox saved by the unit of work |
| Domain events dispatched after `base.SaveChangesAsync` | `ModuleDbContext` | dispatch before the save |
| Event type stored as `AssemblyQualifiedName` | outbox publisher | stable name registry |
| `DateTime` / `Guid.NewGuid()` in events or outbox | `IDomainEvent`, `DomainEventBase`, `OutboxMessage` | `DateTimeOffset` via `TimeProvider`, `Guid.CreateVersion7()` |
| Cross-module non-Contracts project references | csproj `ProjectReference`s | Contracts + architecture test |
| External calls in domain-event handlers | `*DomainEventHandler` using HTTP, S3 or OneSignal | integration event to Notifications or Media |
| Jobs enqueued before commit | `IHopautJobs` / `BackgroundJob.Enqueue` in command handlers | enqueue from a consumer |
| Fire-and-forget tasks | `_ = Task.Run`, unawaited tasks | outbox or Hangfire |
| Handlers registered as singletons | Mediator configuration | `ServiceLifetime.Scoped` |
| Commands or queries not returning `Result` | message definitions | `Result` / `Result<T>` |

## 11. Tests

- **Unit:** validation short-circuits with a `ValidationError`; the unit of work saves exactly once and only on success; caching keys and invalidation.
- **Integration (Testcontainers Postgres):** a command commits aggregate + outbox row atomically, and a failure after the handler persists neither; the relay publishes and marks processed; a failing consumer is retried with backoff and dead-lettered after the maximum; a duplicate delivery produces one effect.
- **Architecture:** no `MediatR` reference; Domain references neither the Mediator runtime nor MassTransit; Application references neither MassTransit nor Hangfire; modules reference only other modules' Contracts; every `IBaseCommand` implements exactly one module marker; every command and query returns a `Result` type.

## Completion criterion

Messaging work is complete only when:
- no module references another module except through Contracts, enforced by tests;
- every integration event is written in the same save as the change that caused it and is relayed with retry and dead-lettering;
- every consumer is idempotent and proven by a duplicate-delivery test;
- MediatR is gone, Mediator diagnostics fail the build, and handlers are scoped;
- no package was adopted without a recorded license check.
