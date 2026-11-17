namespace Hopaut.SharedKernel;

/// <summary>
/// A value object wrapping one primitive. <see cref="Create"/> is the only way to build one; it validates and normalises,
/// and persistence reads stored values through it as well (<c>ValueObjectConverter</c> in BuildingBlocks.Infrastructure).
/// </summary>
public interface ISingleValueObject<TSelf, TValue>
    where TSelf : class, ISingleValueObject<TSelf, TValue>
    where TValue : notnull
{
    TValue Value { get; }

    static abstract Result<TSelf> Create(TValue value);
}
