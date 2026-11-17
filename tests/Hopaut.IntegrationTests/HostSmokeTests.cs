using Hopaut.IntegrationTests.Infrastructure;
using System.Net;

namespace Hopaut.IntegrationTests;

public sealed class HostSmokeTests(HopautApiFactory factory) : IClassFixture<HopautApiFactory>
{
    [Fact]
    public async Task GivenAPostGisDatabase_WhenTheHostStarts_ThenTheHealthEndpointReturnsOk()
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
