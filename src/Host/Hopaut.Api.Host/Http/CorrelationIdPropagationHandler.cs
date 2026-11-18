namespace Hopaut.Api.Host.Http;

/// <summary>Adds the current request's correlation id to every outgoing <see cref="HttpClient"/> call (IHttpClientFactory clients).</summary>
public sealed class CorrelationIdPropagationHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (CorrelationIdMiddleware.Of(accessor.HttpContext) is { } correlationId
            && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
