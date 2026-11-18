using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hopaut.BuildingBlocks.Infrastructure;

/// <summary>
/// EF Core value converter between a single-value value object and its primitive column. Stored values are read back
/// through <see cref="ISingleValueObject{TSelf, TValue}.Create"/>, so an invalid row fails loudly instead of producing an
/// invalid object.
/// </summary>
public sealed class ValueObjectConverter<TValueObject, TValue>()
    : ValueConverter<TValueObject, TValue>(valueObject => valueObject.Value, value => Rehydrate(value))
    where TValueObject : class, ISingleValueObject<TValueObject, TValue>
    where TValue : notnull
{
    // Static abstract members cannot appear in expression trees (CS8927), so the conversion calls through this method.
    private static TValueObject Rehydrate(TValue value)
    {
        var created = TValueObject.Create(value);
        return created.IsSuccess
            ? created.Value
            : throw new InvalidOperationException($"A stored {typeof(TValueObject).Name} is invalid ({created.Error.Code}).");
    }
}
