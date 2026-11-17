using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.IntegrationTests.Infrastructure;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 8.1 (F-064): a rating outside 1–5 is rejected by the <c>RatingValue</c> factory instead of throwing.</summary>
[Collection(HostCollection.Name)]
public sealed class RatingValueEndpointTests(HopautApiFactory factory)
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task GivenARateOutsideOneToFive_WhenARatingIsCreated_ThenItIsAValidationProblem(int rate)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create("rating-rater"));

        // Strongly-typed IDs are bound as { "value": … } until the wire switches to primitives (D-016, plan Step 10.1).
        var body = new
        {
            rate,
            userId = new { value = "rating-host" },
            raterId = new { value = "rating-rater" },
            postId = new { value = 1 },
            feedback = (string?)null,
        };

        // Act
        using var response = await client.PostAsJsonAsync("/api/v1/ratings", body, ct);

        // Assert
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            problem.RootElement.GetProperty("errorCode").GetString().Should().Be("shared.rating_value.out_of_range");
        }
    }
}
