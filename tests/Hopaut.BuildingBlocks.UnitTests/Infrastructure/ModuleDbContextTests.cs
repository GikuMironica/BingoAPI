using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.BuildingBlocks.UnitTests.Infrastructure;

/// <summary>
/// Domain events are dispatched before the database is touched (decision D-004). The context points at a closed port,
/// so any dispatch that happened did so before the save, which then fails to connect.
/// </summary>
public sealed class ModuleDbContextTests
{
    private const string UnreachableDatabase = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=1";
    private static readonly DateTimeOffset Now = new(2026, 1, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GivenRaisedEvents_WhenSaving_ThenEveryEventIncludingOnesRaisedByHandlersIsDispatchedBeforeTheSave()
    {
        // Arrange
        var publisher = new RecordingPublisher();
        await using var context = new VenueContext(publisher);
        var first = Venue.Open(new PostId(1), Now);
        var second = Venue.Open(new PostId(2), Now);
        context.Venues.AddRange(first, second);
        publisher.OnPublished = domainEvent =>
        {
            if (domainEvent is VenueOpened { VenueId.Value: 1 })
            {
                first.Close(Now); // a handler's change raises a further event, dispatched in the next round
            }
        };

        // Act
        var act = () => context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>("the database is unreachable, so only the dispatch can have happened");
        using (new AssertionScope())
        {
            publisher.Published.Select(e => e.GetType().Name).Should().Equal(nameof(VenueOpened), nameof(VenueOpened), nameof(VenueClosed));
            first.DomainEvents.Should().BeEmpty();
            second.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GivenHandlersThatAlwaysRaiseMoreEvents_WhenSaving_ThenItFailsWithoutTouchingTheDatabase()
    {
        // Arrange
        var publisher = new RecordingPublisher();
        await using var context = new VenueContext(publisher);
        var venue = Venue.Open(new PostId(1), Now);
        context.Venues.Add(venue);
        publisher.OnPublished = _ => venue.Close(Now);

        // Act
        var act = () => context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*did not settle*");
    }

    [Fact]
    public void GivenAModuleContext_WhenSavedSynchronously_ThenItIsRejectedBecauseEventsWouldBeSkipped()
    {
        // Arrange
        using var context = new VenueContext(new RecordingPublisher());

        // Act
        var act = () => context.SaveChanges();

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    private sealed record VenueOpened(DateTimeOffset OccurredOnUtc, PostId VenueId) : DomainEventBase(OccurredOnUtc);

    private sealed record VenueClosed(DateTimeOffset OccurredOnUtc, PostId VenueId) : DomainEventBase(OccurredOnUtc);

    private sealed class Venue : AggregateRoot<PostId>
    {
        private Venue() { } // EF Core

        public static Venue Open(PostId id, DateTimeOffset now)
        {
            var venue = new Venue { Id = id };
            venue.Raise(new VenueOpened(now, id));
            return venue;
        }

        public void Close(DateTimeOffset now) => Raise(new VenueClosed(now, Id));
    }

    private sealed class VenueContext(IPublisher publisher)
        : ModuleDbContext(new DbContextOptionsBuilder<VenueContext>().UseNpgsql(UnreachableDatabase).Options, publisher)
    {
        public DbSet<Venue> Venues => Set<Venue>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).HasConversion<StronglyTypedIdConverter<PostId, int>>().ValueGeneratedNever();
            });
    }

    private sealed class RecordingPublisher : IPublisher
    {
        public List<object> Published { get; } = [];

        public Action<object> OnPublished { get; set; } = _ => { };

        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Publish((object)notification, cancellationToken);

        public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
        {
            Published.Add(notification);
            OnPublished(notification);
            return ValueTask.CompletedTask;
        }
    }
}
