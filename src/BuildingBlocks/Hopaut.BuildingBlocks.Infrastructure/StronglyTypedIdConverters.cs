using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hopaut.BuildingBlocks.Infrastructure;

/// <summary>
/// Generic EF Core value converter for strongly-typed IDs backed by int.
/// </summary>
public sealed class StronglyTypedIdConverter<TId> : ValueConverter<TId, int>
    where TId : struct
{
    public StronglyTypedIdConverter()
        : base(
            id => GetValue(id),
            value => Create(value))
    { }

    private static int GetValue(TId id) => ((dynamic)id).Value;
    private static TId Create(int value) => (TId)Activator.CreateInstance(typeof(TId), value)!;
}

/// <summary>
/// Generic EF Core value converter for strongly-typed IDs backed by string.
/// </summary>
public sealed class StronglyTypedStringIdConverter<TId> : ValueConverter<TId, string>
    where TId : struct
{
    public StronglyTypedStringIdConverter()
        : base(
            id => GetValue(id),
            value => Create(value))
    { }

    private static string GetValue(TId id) => ((dynamic)id).Value;
    private static TId Create(string value) => (TId)Activator.CreateInstance(typeof(TId), value)!;
}

/// <summary>
/// Generic EF Core value converter for strongly-typed IDs backed by Guid.
/// </summary>
public sealed class StronglyTypedGuidIdConverter<TId> : ValueConverter<TId, Guid>
    where TId : struct
{
    public StronglyTypedGuidIdConverter()
        : base(
            id => GetValue(id),
            value => Create(value))
    { }

    private static Guid GetValue(TId id) => ((dynamic)id).Value;
    private static TId Create(Guid value) => (TId)Activator.CreateInstance(typeof(TId), value)!;
}
