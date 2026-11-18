using System.Net;
using Hopaut.IntegrationTests.Infrastructure;

namespace Hopaut.IntegrationTests;

[Collection(HostCollection.Name)]
public sealed class HostSmokeTests(HopautApiFactory factory)
{
    [Fact]
    public async Task GivenAPostGisDatabase_WhenTheHostStarts_ThenTheReadinessProbeReturnsOk()
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
