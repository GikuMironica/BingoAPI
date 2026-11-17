using Hopaut.Api.Host.Http;
using Microsoft.AspNetCore.Http;

namespace Hopaut.IntegrationTests;

/// <summary>The correlation id of the incoming request travels with every outgoing IHttpClientFactory call.</summary>
public sealed class CorrelationIdPropagationHandlerTests
{
    [Fact]
    public async Task GivenARequestWithACorrelationId_WhenAnOutgoingCallIsSent_ThenItCarriesTheSameId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "abc-123";
        await new CorrelationIdMiddleware(_ => Task.CompletedTask).InvokeAsync(context);
        var outgoing = new CapturingHandler();
        using var client = new HttpMessageInvoker(
            new CorrelationIdPropagationHandler(new HttpContextAccessor { HttpContext = context }) { InnerHandler = outgoing });

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://payments.example/health");
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        outgoing.Captured!.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Should().Equal("abc-123");
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Captured { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Captured = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
