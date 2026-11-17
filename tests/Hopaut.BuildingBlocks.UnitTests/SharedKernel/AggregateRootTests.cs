using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class AggregateRootTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 9, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GivenAnAggregate_WhenItRaisesEvents_ThenTheyAreExposedInOrder()
    {
        // Arrange
        var aggregate = SampleAggregate.Create(new PostId(42), Now);

        // Act
        aggregate.Rename(Now.AddMinutes(5));

        // Assert
        aggregate.DomainEvents.Select(e => e.GetType()).Should().Equal(typeof(SampleCreated), typeof(SampleRenamed));
    }

    [Fact]
    public void GivenRaisedEvents_WhenCleared_ThenNoEventsRemain()
    {
        // Arrange
        var aggregate = SampleAggregate.Create(new PostId(42), Now);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    private sealed record SampleCreated(DateTimeOffset OccurredOnUtc, PostId PostId) : DomainEventBase(OccurredOnUtc);

    private sealed record SampleRenamed(DateTimeOffset OccurredOnUtc, PostId PostId) : DomainEventBase(OccurredOnUtc);

    private sealed class SampleAggregate : AggregateRoot<PostId>
    {
        private SampleAggregate(PostId id) => Id = id;

        public static SampleAggregate Create(PostId id, DateTimeOffset now)
        {
            var aggregate = new SampleAggregate(id);
            aggregate.Raise(new SampleCreated(now, id));
            return aggregate;
        }

        public void Rename(DateTimeOffset now) => Raise(new SampleRenamed(now, Id));
    }
}
