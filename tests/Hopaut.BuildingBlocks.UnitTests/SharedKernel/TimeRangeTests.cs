using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class TimeRangeTests
{
    private static readonly DateTimeOffset Start = new(2024, 1, 9, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GivenAnEndAfterTheStartInAnotherOffset_WhenCreated_ThenBothAreStoredInUtc()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 9, 20, 0, 0, TimeSpan.FromHours(2));
        var end = new DateTimeOffset(2024, 1, 9, 23, 0, 0, TimeSpan.FromHours(2));

        // Act
        var result = TimeRange.Create(start, end);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Start.Should().Be(Start);
            result.Value.Start.Offset.Should().Be(TimeSpan.Zero);
            result.Value.End!.Value.Offset.Should().Be(TimeSpan.Zero);
            result.Value.Duration.Should().Be(TimeSpan.FromHours(3));
        }
    }

    [Fact]
    public void GivenNoEnd_WhenCreated_ThenTheRangeIsOpen()
    {
        // Act
        var result = TimeRange.Create(Start, null);

        // Assert
        using (new AssertionScope())
        {
            result.Value.End.Should().BeNull();
            result.Value.Duration.Should().BeNull();
            result.Value.Contains(Start.AddYears(1)).Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenAnEndNotAfterTheStart_WhenCreated_ThenItFails(int minutesAfterStart)
    {
        // Act
        var result = TimeRange.Create(Start, Start.AddMinutes(minutesAfterStart));

        // Assert
        result.Error.Code.Should().Be("shared.time_range.end_not_after_start");
    }

    [Theory]
    [InlineData(-1, false, false)]
    [InlineData(0, true, false)]
    [InlineData(59, true, false)]
    [InlineData(60, true, true)]
    public void GivenAnHourLongRange_WhenAskedAboutAMoment_ThenStartAndEndAreInclusiveAndExclusive(
        int minutesAfterStart, bool started, bool ended)
    {
        // Arrange
        var range = TimeRange.Create(Start, Start.AddHours(1)).Value;
        var moment = Start.AddMinutes(minutesAfterStart);

        // Act
        var observed = (range.HasStartedAt(moment), range.HasEndedAt(moment), range.Contains(moment));

        // Assert
        observed.Should().Be((started, ended, started && !ended));
    }
}
