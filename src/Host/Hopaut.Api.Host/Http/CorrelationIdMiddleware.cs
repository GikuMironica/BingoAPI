using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog.Context;

namespace Hopaut.Api.Host.Http;

/// <summary>
/// Gives every request a correlation id: a well-formed incoming <c>X-Correlation-Id</c> is kept, otherwise the W3C trace id
/// (or the request's trace identifier) is used. The id is returned on every response, attached to every log event of the
/// request and forwarded to outgoing HTTP calls by <see cref="CorrelationIdPropagationHandler"/>.
/// </summary>
public sealed partial class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string LogProperty = "CorrelationId";

    private const string ItemKey = "Hopaut.CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = IncomingId(context) ?? Activity.Current?.TraceId.ToHexString() ?? context.TraceIdentifier;
        context.Items[ItemKey] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(LogProperty, correlationId))
        {
            await next(context);
        }
    }

    /// <summary>The correlation id of the request, or <see langword="null"/> outside a request.</summary>
    public static string? Of(HttpContext? context) => context?.Items[ItemKey] as string;

    /// <summary>Only short ids of safe characters are accepted, so a client cannot inject text into logs or headers.</summary>
    private static string? IncomingId(HttpContext context)
    {
        var values = context.Request.Headers[HeaderName];
        return values.Count == 1 && values[0] is { } value && WellFormed().IsMatch(value) ? value : null;
    }

    [GeneratedRegex("^[A-Za-z0-9._:-]{1,64}$")]
    private static partial Regex WellFormed();
}
