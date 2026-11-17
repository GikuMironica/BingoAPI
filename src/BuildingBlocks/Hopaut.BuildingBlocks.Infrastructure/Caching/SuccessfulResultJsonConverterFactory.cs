using System.Text.Json;
using System.Text.Json.Serialization;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Lets the query cache store the successful <see cref="Result"/> / <see cref="Result{TValue}"/> responses that
/// <c>CachingBehavior</c> caches: only the value is written (<c>{"value":…}</c>), and reading rebuilds a success.
/// Failures are never cached, so writing one is a programming error.
/// </summary>
public sealed class SuccessfulResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(Result)
        || (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(Result))
        {
            return new SuccessfulResultJsonConverter();
        }

        var converterType = typeof(SuccessfulValueResultJsonConverter<>).MakeGenericType(typeToConvert.GenericTypeArguments[0]);
        return Activator.CreateInstance(converterType) as JsonConverter
            ?? throw new InvalidOperationException($"Could not create a JSON converter for {typeToConvert}.");
    }
}

internal sealed class SuccessfulResultJsonConverter : JsonConverter<Result>
{
    public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("A cached result must be a JSON object.");
        }

        reader.Skip();
        return Result.Success();
    }

    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        SuccessfulResultJson.EnsureSuccess(value);
        writer.WriteStartObject();
        writer.WriteEndObject();
    }
}

internal sealed class SuccessfulValueResultJsonConverter<TValue> : JsonConverter<Result<TValue>>
{
    public override Result<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("A cached result must be a JSON object.");
        }

        TValue? value = default;
        var hasValue = false;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var isValue = reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(SuccessfulResultJson.ValueProperty);
            reader.Read();
            if (isValue)
            {
                value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                hasValue = true;
            }
            else
            {
                reader.Skip();
            }
        }

        return hasValue
            ? Result<TValue>.Success(value!)
            : throw new JsonException($"A cached result must contain \"{SuccessfulResultJson.ValueProperty}\".");
    }

    public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options)
    {
        SuccessfulResultJson.EnsureSuccess(value);
        writer.WriteStartObject();
        writer.WritePropertyName(SuccessfulResultJson.ValueProperty);
        JsonSerializer.Serialize(writer, value.Value, options);
        writer.WriteEndObject();
    }
}

internal static class SuccessfulResultJson
{
    public const string ValueProperty = "value";

    public static void EnsureSuccess(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed results are never cached (error '{result.Error.Code}').");
        }
    }
}
