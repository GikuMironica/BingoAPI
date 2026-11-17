using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Api.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Hopaut.BuildingBlocks.UnitTests.Api;

public sealed class HopautExceptionHandlerTests
{
    [Fact]
    public async Task GivenAnUnhandledException_WhenHandled_ThenA500ProblemWithoutInternalsIsWritten()
    {
        // Arrange
        await using var services = new ServiceCollection().AddLogging().AddHopautProblemDetails().BuildServiceProvider();
        var handler = services.GetRequiredService<IExceptionHandler>();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/posts/7";
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("connection string Password=hunter2");

        // Act
        var handled = await handler.TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        // Assert
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        using (new AssertionScope())
        {
            handled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            context.Response.ContentType.Should().StartWith("application/problem+json");
            json.RootElement.GetProperty("errorCode").GetString().Should().Be(HopautProblemDetails.UnexpectedErrorCode);
            json.RootElement.TryGetProperty("traceId", out _).Should().BeTrue();
            body.Should().NotContain("hunter2").And.NotContain(nameof(InvalidOperationException));
        }
    }

    [Theory]
    [InlineData(StatusCodes.Status404NotFound, "general.not_found")]
    [InlineData(StatusCodes.Status429TooManyRequests, "general.too_many_requests")]
    [InlineData(StatusCodes.Status503ServiceUnavailable, HopautProblemDetails.UnexpectedErrorCode)]
    [InlineData(418, "general.http_418")]
    public void GivenAFrameworkStatus_WhenNoDomainErrorExists_ThenAStableFallbackCodeIsUsed(int status, string expected)
    {
        // Act
        var code = HopautProblemDetails.FallbackErrorCode(status);

        // Assert
        code.Should().Be(expected);
    }
}
