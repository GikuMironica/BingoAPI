using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hopaut.SharedKernel.Serialization;

/// <summary>
/// Serializes every <see cref="IStronglyTypedId{TSelf, TValue}"/> as its primitive value, also as a dictionary key, and
/// rejects invalid keys when reading. Registered explicitly on the serializer options of a boundary (HTTP JSON in plan
/// Step 10.1, the module outbox in Step 9.1) rather than by attribute, so endpoints keep today's JSON shape until their
/// Phase 10 sync (decision D-016 in TODO.md).
/// </summary>
public sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => ValueTypeOf(typeToConvert) is not null;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = ValueTypeOf(typeToConvert)
            ?? throw new ArgumentException($"{typeToConvert} is not a strongly-typed ID.", nameof(typeToConvert));
        var converterType = typeof(StronglyTypedIdJsonConverter<,>).MakeGenericType(typeToConvert, valueType);

        return Activator.CreateInstance(converterType) as JsonConverter
            ?? throw new InvalidOperationException($"Could not create a JSON converter for {typeToConvert}.");
    }

    private static Type? ValueTypeOf(Type type) =>
        type.IsValueType
            ? type.GetInterfaces()
                .Where(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(IStronglyTypedId<,>)
                            && i.GenericTypeArguments[0] == type)
                .Select(i => i.GenericTypeArguments[1])
                .SingleOrDefault()
            : null;
}

internal sealed class StronglyTypedIdJsonConverter<TId, TValue> : JsonConverter<TId>
    where TId : struct, IStronglyTypedId<TId, TValue>
    where TValue : notnull
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        StronglyTypedId.TryCreate<TId, TValue>(JsonSerializer.Deserialize<TValue>(ref reader, options), out var id)
            ? id
            : throw new JsonException($"The JSON value is not a valid {typeof(TId).Name}.");

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.Value, options);

    public override TId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.TryParse(reader.GetString(), CultureInfo.InvariantCulture, out var id)
            ? id
            : throw new JsonException($"The JSON property name is not a valid {typeof(TId).Name}.");

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WritePropertyName(Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty);
}
