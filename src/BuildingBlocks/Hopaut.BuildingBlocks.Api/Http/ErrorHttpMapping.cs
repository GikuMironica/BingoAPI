using Hopaut.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Hopaut.BuildingBlocks.Api.Http;

/// <summary>The only place where <see cref="ErrorType"/> becomes an HTTP status (clean-architecture-ddd skill §5).</summary>
public static class ErrorHttpMapping
{
    public const string ErrorCodeExtension = "errorCode";

    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Rule => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unmapped error type."),
    };

    /// <summary>RFC 9457 problem for an expected failure: status from the error type, <c>errorCode</c> extension, field errors for validation.</summary>
    public static ProblemHttpResult ToProblem(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error == Error.None)
        {
            throw new ArgumentException("Error.None is not a failure.", nameof(error));
        }

        ProblemDetails problem = error is ValidationError validation
            ? new HttpValidationProblemDetails(validation.Errors.ToDictionary(e => e.Key, e => e.Value))
            : new ProblemDetails();
        problem.Status = error.Type.ToStatusCode();
        problem.Detail = error.Description;
        problem.Extensions[ErrorCodeExtension] = error.Code;
        return TypedResults.Problem(problem);
    }

    public static ProblemHttpResult ToProblem(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsFailure
            ? result.Error.ToProblem()
            : throw new InvalidOperationException("A successful result cannot be turned into a problem.");
    }
}
