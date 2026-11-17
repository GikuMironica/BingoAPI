using System.Net;
using System.Net.Http.Headers;
using AwesomeAssertions.Execution;
using Hopaut.Api.Host.Http;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Identity.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 7.7: JWT bearer authentication, health probes, correlation ids, the dashboard filter and startup validation.</summary>
[Collection(HostCollection.Name)]
public sealed class HostHardeningTests(HopautApiFactory factory)
{
    private const string ProtectedPath = "/api/v1/ratings/user/hardening-user";

    [Theory]
    [InlineData("missing")]
    [InlineData("wrong-issuer")]
    [InlineData("wrong-audience")]
    [InlineData("expired")]
    [InlineData("wrong-key")]
    public async Task GivenNoValidToken_WhenAProtectedEndpointIsCalled_ThenItReturns401(string token)
    {
        // Arrange
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, ProtectedPath);
        var jwt = token switch
        {
            "missing" => null,
            "wrong-issuer" => TestJwt.Create("hardening-user", issuer: "https://issuer.example"),
            "wrong-audience" => TestJwt.Create("hardening-user", audience: "another-client"),
            "expired" => TestJwt.Create("hardening-user", expires: DateTimeOffset.UtcNow.AddMinutes(-5)),
            "wrong-key" => TestJwt.Create("hardening-user", secret: "a-different-signing-key-0123456789abcdef"),
            _ => throw new ArgumentOutOfRangeException(nameof(token), token, null),
        };
        if (jwt is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAValidToken_WhenAProtectedEndpointIsCalled_ThenItSucceeds()
    {
        // Arrange
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, ProtectedPath);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create("hardening-user"));

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task GivenTheHost_WhenAProbeIsCalled_ThenItReturns200(string path)
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GivenNoCorrelationId_WhenTheHostAnswers_ThenTheResponseCarriesAGeneratedOne()
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        // Assert
        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Should().ContainSingle()
            .Which.Should().MatchRegex("^[A-Za-z0-9._:-]{1,64}$");
    }

    [Fact]
    public async Task GivenAWellFormedCorrelationId_WhenARequestIsRejected_ThenTheSameIdIsReturned()
    {
        // Arrange
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, ProtectedPath);
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, "mobile-7f3a.42");

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Should().Equal("mobile-7f3a.42");
        }
    }

    [Fact]
    public async Task GivenAMalformedCorrelationId_WhenSent_ThenItIsReplaced()
    {
        // Arrange
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, "<script>alert(1)</script>");

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Should().ContainSingle()
            .Which.Should().NotContain("<");
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData(HopautRoles.User, HttpStatusCode.Forbidden)]
    [InlineData(HopautRoles.Admin, HttpStatusCode.OK)]
    [InlineData(HopautRoles.SuperAdmin, HttpStatusCode.OK)]
    public async Task GivenACaller_WhenTheHangfireDashboardIsRequested_ThenOnlyAdministratorsGetIn(string? role, HttpStatusCode expected)
    {
        // Arrange
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/admin/hangfire");
        if (role is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create("dashboard-user", [role]));
        }

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public void GivenScopeAndBuildValidation_WhenTheHostStarts_ThenEveryRegistrationIsValid()
    {
        // Arrange: the validation the Host enables in Development
        using var validating = factory.WithWebHostBuilder(builder => builder.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        }));

        // Act
        var act = () => validating.Services;

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GivenNoJwtSigningKey_WhenTheHostStarts_ThenStartupFails()
    {
        // Arrange
        using var withoutKey = factory.WithWebHostBuilder(builder => builder.UseSetting("JwtSettings:Secret", string.Empty));

        // Act
        var act = () => withoutKey.Services;

        // Assert
        var thrown = act.Should().Throw<Exception>().Which;
        SelfAndInner(thrown).OfType<OptionsValidationException>().Should().Contain(e => e.OptionsType == typeof(JwtSettings));
    }

    private static IEnumerable<Exception> SelfAndInner(Exception exception) =>
        exception is AggregateException aggregate
            ? [aggregate, .. aggregate.InnerExceptions.SelectMany(SelfAndInner)]
            : exception.InnerException is null ? [exception] : [exception, .. SelfAndInner(exception.InnerException)];
}
