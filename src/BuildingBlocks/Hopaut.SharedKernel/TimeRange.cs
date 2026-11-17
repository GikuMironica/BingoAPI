namespace Hopaut.SharedKernel;

/// <summary>A start and an optional end, both in UTC; the end lies after the start.</summary>
public sealed record TimeRange
{
    private TimeRange(DateTimeOffset start, DateTimeOffset? end) => (Start, End) = (start, end);

    public DateTimeOffset Start { get; }

    public DateTimeOffset? End { get; }

    public TimeSpan? Duration => End - Start;

    public static Result<TimeRange> Create(DateTimeOffset start, DateTimeOffset? end)
    {
        if (end is { } until && until <= start)
            return TimeRangeErrors.EndNotAfterStart;

        return new TimeRange(start.ToUniversalTime(), end?.ToUniversalTime());
    }

    public bool HasStartedAt(DateTimeOffset now) => now >= Start;

    public bool HasEndedAt(DateTimeOffset now) => End is { } end && now >= end;

    public bool Contains(DateTimeOffset now) => HasStartedAt(now) && !HasEndedAt(now);
}

public static class TimeRangeErrors
{
    public static readonly Error EndNotAfterStart =
        Error.Validation("shared.time_range.end_not_after_start", "The end must be after the start.");
}
