namespace Hopaut.SharedKernel;

/// <summary>
/// Represents a geographic coordinate pair (WGS-84).
/// </summary>
public readonly record struct Coordinates(double Latitude, double Longitude)
{
    public static Coordinates From(double latitude, double longitude) => new(latitude, longitude);

    /// <summary>
    /// Validates that the coordinate values are within valid WGS-84 ranges.
    /// </summary>
    public bool IsValid => Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;
}

/// <summary>
/// Value object representing a rating score (1–5 inclusive).
/// </summary>
public readonly record struct RatingValue
{
    public int Value { get; }

    public RatingValue(int value)
    {
        if (value is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5.");
        Value = value;
    }

    public static RatingValue From(int value) => new(value);
    public static implicit operator int(RatingValue r) => r.Value;
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Value object for entrance fee / price on an event.
/// </summary>
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money From(decimal amount, string currency) => new(amount, currency);
    public static Money Zero(string currency = "EUR") => new(0, currency);
}
