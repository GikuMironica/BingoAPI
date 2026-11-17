using System.Globalization;

namespace Hopaut.SharedKernel;

/// <summary>How many guests an event admits: a positive number.</summary>
public sealed record Capacity : ISingleValueObject<Capacity, int>
{
    private Capacity(int value) => Value = value;

    public int Value { get; }

    public static Result<Capacity> Create(int value) => value > 0 ? new Capacity(value) : CapacityErrors.NotPositive;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>A search radius in meters from <see cref="Min"/> to <see cref="Max"/> (the legacy 1–15 km feed bounds).</summary>
public sealed record RadiusMeters : ISingleValueObject<RadiusMeters, int>
{
    public const int Min = 1_000;
    public const int Max = 15_000;

    private RadiusMeters(int value) => Value = value;

    /// <summary>10 km, the radius the feed uses when the client sends none.</summary>
    public static RadiusMeters Default { get; } = new(10_000);

    public int Value { get; }

    public static Result<RadiusMeters> Create(int value) =>
        value is >= Min and <= Max ? new RadiusMeters(value) : RadiusMetersErrors.OutOfRange;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

public static class CapacityErrors
{
    public static readonly Error NotPositive =
        Error.Validation("shared.capacity.not_positive", "The capacity must be at least 1.");
}

public static class RadiusMetersErrors
{
    public static readonly Error OutOfRange =
        Error.Validation("shared.radius.out_of_range", $"The radius must be between {RadiusMeters.Min} and {RadiusMeters.Max} meters.");
}
