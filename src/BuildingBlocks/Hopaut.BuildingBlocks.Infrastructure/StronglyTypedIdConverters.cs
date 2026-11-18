using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hopaut.BuildingBlocks.Infrastructure;

/// <summary>
/// EF Core value converter between a strongly-typed ID and its primitive column. Stored keys are wrapped as is
/// (<see cref="IStronglyTypedId{TSelf, TValue}.From"/>); validation applies only at the API and serialization edges.
/// </summary>
public sealed class StronglyTypedIdConverter<TId, TValue>() : ValueConverter<TId, TValue>(id => id.Value, value => Wrap(value))
    where TId : struct, IStronglyTypedId<TId, TValue>
    where TValue : notnull
{
    // Static abstract members cannot appear in expression trees (CS8927), so the conversion calls through this method.
    private static TId Wrap(TValue value) => TId.From(value);
}
