using System.Globalization;

namespace Hopaut.SharedKernel;

/// <summary>A rating score from <see cref="Min"/> to <see cref="Max"/>.</summary>
public sealed record RatingValue : ISingleValueObject<RatingValue, int>
{
    public const int Min = 1;
    public const int Max = 5;

    private RatingValue(int value) => Value = value;

    public int Value { get; }

    public static Result<RatingValue> Create(int value) =>
        value is >= Min and <= Max ? new RatingValue(value) : RatingValueErrors.OutOfRange;

    /// <summary>One-way conversion for arithmetic and SQL aggregates (the reputation sum); creation always goes through <see cref="Create"/>.</summary>
    public static implicit operator int(RatingValue rating) => rating.Value;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

public static class RatingValueErrors
{
    public static readonly Error OutOfRange =
        Error.Validation("shared.rating_value.out_of_range", $"A rating must be between {RatingValue.Min} and {RatingValue.Max}.");
}
