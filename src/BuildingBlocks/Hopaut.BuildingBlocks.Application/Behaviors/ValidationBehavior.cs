using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Runs every FluentValidation validator of the message. Failures short-circuit the pipeline with a
/// <see cref="ValidationError"/> keyed by camelCase field path; the handler never runs and nothing throws.
/// </summary>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : IFailureFactory<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        var errors = failures
            .GroupBy(failure => ToFieldPath(failure.PropertyName), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        return TResponse.Failure(new ValidationError(errors));
    }

    /// <summary>"Location.City" → "location.city", "TagNames[0]" → "tagNames[0]": the wire uses camelCase JSON names.</summary>
    private static string ToFieldPath(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
