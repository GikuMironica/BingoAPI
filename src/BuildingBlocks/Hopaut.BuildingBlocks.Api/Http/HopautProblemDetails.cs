using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hopaut.BuildingBlocks.Api.Http;

public static class HopautProblemDetails
{
    public const string TraceIdExtension = "traceId";
    public const string UnexpectedErrorCode = "general.unexpected";

    /// <summary>
    /// Every problem response carries <c>errorCode</c> and <c>traceId</c> (rest-api-standards skill §4); unhandled exceptions
    /// become a 500 problem without internals.
    /// </summary>
    public static IServiceCollection AddHopautProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            problem.Instance ??= context.HttpContext.Request.Path.Value;
            problem.Extensions.TryAdd(TraceIdExtension, Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            problem.Extensions.TryAdd(ErrorHttpMapping.ErrorCodeExtension, FallbackErrorCode(problem.Status ?? context.HttpContext.Response.StatusCode));
        });
        services.AddExceptionHandler<HopautExceptionHandler>();
        return services;
    }

    /// <summary>Stable code for problems that do not come from a domain <c>Error</c> (framework 4xx, unhandled 5xx).</summary>
    public static string FallbackErrorCode(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "general.bad_request",
        StatusCodes.Status401Unauthorized => "general.unauthorized",
        StatusCodes.Status403Forbidden => "general.forbidden",
        StatusCodes.Status404NotFound => "general.not_found",
        StatusCodes.Status405MethodNotAllowed => "general.method_not_allowed",
        StatusCodes.Status409Conflict => "general.conflict",
        StatusCodes.Status415UnsupportedMediaType => "general.unsupported_media_type",
        StatusCodes.Status422UnprocessableEntity => "general.unprocessable",
        StatusCodes.Status429TooManyRequests => "general.too_many_requests",
        >= 500 => UnexpectedErrorCode,
        _ => $"general.http_{status}",
    };
}

/// <summary>Logs the exception and answers with a generic 500 problem; never exposes messages, types or stack traces.</summary>
public sealed partial class HopautExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<HopautExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogUnhandledException(logger, exception, httpContext.Request.Method, httpContext.Request.Path.Value ?? "/");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Extensions = { [ErrorHttpMapping.ErrorCodeExtension] = HopautProblemDetails.UnexpectedErrorCode },
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception while processing {Method} {Path}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string method, string path);
}
