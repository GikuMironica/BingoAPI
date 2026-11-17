using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Hopaut.SharedKernel;

/// <summary>
/// An identifier wrapping a primitive key so keys of different aggregates cannot be mixed up. Parsing (route values) and
/// JSON reading accept only valid keys (<see cref="StronglyTypedId.IsValid{TValue}"/>); <see cref="From"/> wraps a stored
/// key as is.
/// </summary>
public interface IStronglyTypedId<TSelf, TValue> : IParsable<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf, TValue>
    where TValue : notnull
{
    TValue Value { get; }

    static abstract TSelf From(TValue value);
}

/// <summary>Rules shared by every strongly-typed ID: int keys are positive, string keys are not blank, Guid keys are not empty.</summary>
public static class StronglyTypedId
{
    public static bool IsValid<TValue>([NotNullWhen(true)] TValue? value) => value switch
    {
        int number => number > 0,
        string text => !string.IsNullOrWhiteSpace(text),
        Guid guid => guid != Guid.Empty,
        _ => false,
    };

    public static bool TryCreate<TId, TValue>([NotNullWhen(true)] TValue? value, out TId id)
        where TId : struct, IStronglyTypedId<TId, TValue>
        where TValue : notnull
    {
        if (IsValid(value))
        {
            id = TId.From(value);
            return true;
        }

        id = default;
        return false;
    }

    public static bool TryParseInt32<TId>([NotNullWhen(true)] string? s, out TId id)
        where TId : struct, IStronglyTypedId<TId, int>
    {
        id = default;
        return int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && TryCreate(value, out id);
    }

    public static bool TryParseString<TId>([NotNullWhen(true)] string? s, out TId id)
        where TId : struct, IStronglyTypedId<TId, string> =>
        TryCreate(s, out id);

    public static bool TryParseGuid<TId>([NotNullWhen(true)] string? s, out TId id)
        where TId : struct, IStronglyTypedId<TId, Guid>
    {
        id = default;
        return Guid.TryParse(s, out var value) && TryCreate(value, out id);
    }

    public static TId Parse<TId>(string s, IFormatProvider? provider)
        where TId : struct, IParsable<TId>
    {
        ArgumentNullException.ThrowIfNull(s);
        return TId.TryParse(s, provider, out var id) ? id : throw new FormatException($"'{s}' is not a valid {typeof(TId).Name}.");
    }
}
