# Clean Architecture + DDD patterns (Hopaut)

Reference implementations for the rules in [SKILL.md](../SKILL.md). Adapt names; keep the shape. All snippets target .NET 10 / C# 14 with nullable enabled.

## Contents
1. Error and Result
2. Strongly-typed IDs
3. Value objects
4. Entity and AggregateRoot base classes
5. Aggregate with a state machine
6. Domain events and error catalog
7. Command + handler
8. Query + read port
9. Repository with sequence-assigned IDs
10. EF Core configuration
11. Tests

## 1. Error and Result

```csharp
namespace Hopaut.SharedKernel;

public enum ErrorType { Failure, Validation, NotFound, Conflict, Rule, Unauthorized, Forbidden }

public record Error(string Code, string Description, ErrorType Type)
{
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error Rule(string code, string description) => new(code, description, ErrorType.Rule);
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);
    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
}

public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error("validation.failed", "One or more validation errors occurred.", ErrorType.Validation);

// Lets pipeline behaviors build a failed TResponse without reflection.
public interface IFailureFactory<TSelf> where TSelf : IFailureFactory<TSelf>
{
    static abstract TSelf Failure(Error error);
}

// Lets the unit-of-work behavior check success for any result type.
public interface IResultBase
{
    Error? Error { get; }
    bool IsSuccess { get; }
}

public sealed class Result : IResultBase, IFailureFactory<Result>
{
    private Result(Error? error) => Error = error;

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => Error is not null;

    public static Result Success() => new(null);
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}

public sealed class Result<T> : IResultBase, IFailureFactory<Result<T>>
{
    private readonly T? _value;

    private Result(T value) => _value = value;
    private Result(Error error) => Error = error;

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => Error is not null;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
```

## 2. Strongly-typed IDs

```csharp
// int-backed (existing keys; wire stays an integer)
public readonly record struct PostId(int Value) : IParsable<PostId>
{
    public static PostId Parse(string s, IFormatProvider? provider) =>
        TryParse(s, provider, out var id) ? id : throw new FormatException($"'{s}' is not a valid {nameof(PostId)}.");

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out PostId result)
    {
        if (int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value > 0)
        {
            result = new PostId(value);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

// Guid-backed (new aggregates only)
public readonly record struct MediaAssetId(Guid Value)
{
    public static MediaAssetId New() => new(Guid.CreateVersion7());
}
```

## 3. Value objects

```csharp
public sealed record Currency
{
    private Currency(string code) => Code = code;

    public string Code { get; }

    public static Result<Currency> Create(string? code)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        return normalized is { Length: 3 } && normalized.All(char.IsAsciiLetterUpper)
            ? new Currency(normalized)
            : MoneyErrors.InvalidCurrency;
    }
}

public sealed record Money
{
    private Money(decimal amount, Currency currency) => (Amount, Currency) = (amount, currency);

    public decimal Amount { get; }
    public Currency Currency { get; }

    public static Result<Money> Create(decimal amount, Currency currency) =>
        amount < 0 ? MoneyErrors.Negative : new Money(decimal.Round(amount, 2), currency);
}

public sealed record TimeRange
{
    private TimeRange(DateTimeOffset start, DateTimeOffset? end) => (Start, End) = (start, end);

    public DateTimeOffset Start { get; }
    public DateTimeOffset? End { get; }

    public static Result<TimeRange> Create(DateTimeOffset start, DateTimeOffset? end) =>
        end is { } e && e <= start
            ? TimeRangeErrors.EndNotAfterStart
            : new TimeRange(start.ToUniversalTime(), end?.ToUniversalTime());

    public bool HasStartedAt(DateTimeOffset now) => now >= Start;
}
```

## 4. Entity and AggregateRoot base classes

```csharp
public abstract class Entity<TId> where TId : struct
{
    public TId Id { get; protected set; }

    private bool IsTransient => Id.Equals(default(TId));

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other
        && other.GetType() == GetType()
        && (ReferenceEquals(this, other) || (!IsTransient && !other.IsTransient && Id.Equals(other.Id)));

    public override int GetHashCode() => IsTransient ? base.GetHashCode() : HashCode.Combine(GetType(), Id);
}

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
```

## 5. Aggregate with a state machine

```csharp
public enum AttendanceStatus { Pending, Accepted, Rejected, Cancelled }

public sealed class Participation : AggregateRoot<ParticipationId>
{
    private Participation() { } // EF Core

    private Participation(ParticipationId id, PostId postId, UserId userId, DateTimeOffset requestedAt)
    {
        Id = id;
        PostId = postId;
        UserId = userId;
        RequestedAt = requestedAt;
        Status = AttendanceStatus.Pending;
    }

    public PostId PostId { get; private set; }
    public UserId UserId { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }
    public uint Version { get; private set; }

    public static Participation Request(ParticipationId id, PostId postId, UserId userId, DateTimeOffset now)
    {
        var participation = new Participation(id, postId, userId, now);
        participation.Raise(new AttendanceRequestedDomainEvent(Guid.CreateVersion7(), now, id, postId, userId));
        return participation;
    }

    public Result Accept(DateTimeOffset now) => Decide(AttendanceStatus.Accepted, now);

    public Result Reject(DateTimeOffset now) => Decide(AttendanceStatus.Rejected, now);

    private Result Decide(AttendanceStatus decision, DateTimeOffset now)
    {
        if (Status is not AttendanceStatus.Pending)
            return ParticipationErrors.NotPending(Id);

        Status = decision;
        DecidedAt = now;
        Raise(new AttendanceDecidedDomainEvent(Guid.CreateVersion7(), now, Id, PostId, UserId, decision));
        return Result.Success();
    }
}
```

## 6. Domain events and error catalog

```csharp
public interface IDomainEvent : INotification // Mediator.Abstractions
{
    Guid EventId { get; }
    DateTimeOffset OccurredOnUtc { get; }
}

public sealed record AttendanceRequestedDomainEvent(
    Guid EventId, DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : IDomainEvent;

public sealed record AttendanceDecidedDomainEvent(
    Guid EventId, DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId,
    AttendanceStatus Decision) : IDomainEvent;

public static class ParticipationErrors
{
    public static Error NotFound(ParticipationId id) =>
        Error.NotFound("attendance.participation.not_found", $"Participation {id} was not found.");

    public static Error NotPending(ParticipationId id) =>
        Error.Conflict("attendance.participation.not_pending", $"Participation {id} has already been decided.");

    public static readonly Error NotPostHost =
        Error.Forbidden("attendance.participation.not_post_host", "Only the post host can decide attendance.");
}
```

## 7. Command + handler

```csharp
public sealed record AcceptAttendanceCommand(ParticipationId ParticipationId, UserId ActingUserId)
    : ICommand<Result>, IAttendanceCommand; // module marker selects the module's UnitOfWork behavior

// Public: the Mediator source generator in the Host must reference it.
public sealed class AcceptAttendanceCommandHandler(
    IParticipationRepository participations,
    IPostHostQueries postHosts, // Posts.Contracts read port
    TimeProvider clock) : ICommandHandler<AcceptAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(AcceptAttendanceCommand command, CancellationToken cancellationToken)
    {
        var participation = await participations.GetByIdAsync(command.ParticipationId, cancellationToken);
        if (participation is null)
            return ParticipationErrors.NotFound(command.ParticipationId);

        if (!await postHosts.IsHostAsync(participation.PostId, command.ActingUserId, cancellationToken))
            return ParticipationErrors.NotPostHost;

        return participation.Accept(clock.GetUtcNow());
    }
}

public sealed class AcceptAttendanceCommandValidator : AbstractValidator<AcceptAttendanceCommand>
{
    public AcceptAttendanceCommandValidator() => RuleFor(c => c.ParticipationId.Value).GreaterThan(0);
}
```

## 8. Query + read port

```csharp
public sealed record PostDetailsDto(int Id, string Title, DateTimeOffset StartsAt, DateTimeOffset? EndsAt);

public sealed record GetPostDetailsQuery(PostId PostId) : IQuery<Result<PostDetailsDto>>;

public interface IPostReadModel // Application port
{
    Task<PostDetailsDto?> GetDetailsAsync(PostId postId, CancellationToken cancellationToken);
}

public sealed class GetPostDetailsQueryHandler(IPostReadModel posts)
    : IQueryHandler<GetPostDetailsQuery, Result<PostDetailsDto>>
{
    public async ValueTask<Result<PostDetailsDto>> Handle(GetPostDetailsQuery query, CancellationToken cancellationToken)
    {
        var details = await posts.GetDetailsAsync(query.PostId, cancellationToken);
        return details is null ? PostErrors.NotFound(query.PostId) : details;
    }
}

// Infrastructure adapter
internal sealed class PostReadModel(PostsModuleDbContext db) : IPostReadModel
{
    public Task<PostDetailsDto?> GetDetailsAsync(PostId postId, CancellationToken cancellationToken) =>
        db.Posts
            .AsNoTracking()
            .Where(p => p.Id == postId)
            .Select(p => new PostDetailsDto(p.Id.Value, p.Title.Value, p.Schedule.Start, p.Schedule.End))
            .SingleOrDefaultAsync(cancellationToken);
}
```

## 9. Repository with sequence-assigned IDs

```csharp
public interface IParticipationRepository // Application port
{
    Task<ParticipationId> NextIdAsync(CancellationToken cancellationToken);
    Task<Participation?> GetByIdAsync(ParticipationId id, CancellationToken cancellationToken);
    void Add(Participation participation);
}

internal sealed class ParticipationRepository(AttendanceModuleDbContext db) : IParticipationRepository
{
    public async Task<ParticipationId> NextIdAsync(CancellationToken cancellationToken) =>
        new(await db.Database
            .SqlQuery<int>($"SELECT nextval('attendance.participation_ids')::int AS \"Value\"")
            .SingleAsync(cancellationToken));

    public Task<Participation?> GetByIdAsync(ParticipationId id, CancellationToken cancellationToken) =>
        db.Participations.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(Participation participation) => db.Participations.Add(participation);
}
```

The migration that introduces the sequence must start it above the current `MAX(id)` (`setval`) so existing rows keep their IDs.

## 10. EF Core configuration

```csharp
internal sealed class ParticipationConfiguration : IEntityTypeConfiguration<Participation>
{
    public void Configure(EntityTypeBuilder<Participation> builder)
    {
        builder.ToTable("participations");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new ParticipationId(value))
            .ValueGeneratedNever();
        builder.Property(p => p.PostId).HasConversion(id => id.Value, value => new PostId(value));
        builder.Property(p => p.UserId).HasConversion(id => id.Value, value => new UserId(value)).HasMaxLength(450);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(16); // only if the column change is planned
        builder.Property(p => p.Version).IsRowVersion(); // Npgsql maps uint row versions to xmin
        builder.HasIndex(p => new { p.PostId, p.UserId }).IsUnique();
        builder.Ignore(p => p.DomainEvents);
    }
}

// Value object as complex type (EF Core 10 supports optional, nested and struct complex types)
builder.ComplexProperty(e => e.EntranceFee, fee =>
{
    fee.Property(m => m.Amount).HasColumnName("entrance_fee_amount").HasPrecision(10, 2);
    fee.ComplexProperty(m => m.Currency, c => c.Property(x => x.Code).HasColumnName("entrance_fee_currency").HasMaxLength(3));
});
```

## 11. Tests

```csharp
public sealed class ParticipationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);

    private static Participation Pending() =>
        Participation.Request(new ParticipationId(1), new PostId(7), new UserId("user-1"), Now);

    [Fact]
    public void Accept_WhenPending_AcceptsAndRaisesDecidedEvent()
    {
        var participation = Pending();

        var result = participation.Accept(Now.AddMinutes(5));

        Assert.True(result.IsSuccess);
        Assert.Equal(AttendanceStatus.Accepted, participation.Status);
        Assert.Contains(participation.DomainEvents, e => e is AttendanceDecidedDomainEvent);
    }

    [Fact]
    public void Accept_WhenAlreadyRejected_ReturnsNotPendingConflict()
    {
        var participation = Pending();
        participation.Reject(Now);

        var result = participation.Accept(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("attendance.participation.not_pending", result.Error.Code);
    }
}

public sealed class AcceptAttendanceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenActingUserIsNotHost_ReturnsForbidden()
    {
        var participation = Participation.Request(new ParticipationId(1), new PostId(7), new UserId("guest"), DateTimeOffset.UnixEpoch);
        var repository = Substitute.For<IParticipationRepository>();
        repository.GetByIdAsync(participation.Id, Arg.Any<CancellationToken>()).Returns(participation);
        var hosts = Substitute.For<IPostHostQueries>();
        hosts.IsHostAsync(participation.PostId, Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new AcceptAttendanceCommandHandler(repository, hosts, new FakeTimeProvider());

        var result = await handler.Handle(new AcceptAttendanceCommand(participation.Id, new UserId("intruder")), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(AttendanceStatus.Pending, participation.Status);
    }
}
```
