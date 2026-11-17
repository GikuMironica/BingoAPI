using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Infrastructure;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 8.2: Posts endpoints on today's wire (D-007) backed by the refactored aggregate.</summary>
[Collection(HostCollection.Name)]
public sealed class PostsEndpointTests(HopautApiFactory factory)
{
    [Fact]
    public async Task GivenABodyNamingAnotherHost_WhenAPostIsCreated_ThenTheCallerIsTheHost()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var caller = "posts-caller-" + Guid.NewGuid().ToString("N");
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(caller));

        // Act
        using var response = await client.PostAsJsonAsync("/api/v1/posts", Body(userId: "someone-else", title: "Rooftop party"), ct);

        // Assert
        using var created = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var postId = PostId.From(created.RootElement.GetProperty("postId").GetInt32());
        await using var scope = factory.Services.CreateAsyncScope();
        var post = await scope.ServiceProvider.GetRequiredService<PostsModuleDbContext>().Posts
            .Include(p => p.Event)
            .SingleAsync(p => p.Id == postId, ct);
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            post.UserId.Should().Be(UserId.From(caller));
            post.Status.Should().Be(PostStatus.Active);
            post.Event.Title.Value.Should().Be("Rooftop party");
        }
    }

    [Fact]
    public async Task GivenInvalidFields_WhenAPostIsCreated_ThenEveryFieldIsReportedInOneValidationProblem()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create("posts-invalid"));

        // Act
        using var response = await client.PostAsJsonAsync("/api/v1/posts", Body(userId: "posts-invalid", title: "", description: "short"), ct);

        // Assert
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            problem.RootElement.GetProperty("errors").EnumerateObject().Select(e => e.Name)
                .Should().Contain(["event.title", "event.description"]);
        }
    }

    // Today's request shape: the command itself, with strongly-typed IDs as { "value": … } (D-016, plan Step 10.4).
    private static object Body(string userId, string title, string description = "Bring snacks and good vibes") => new
    {
        eventTime = new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero),
        endTime = new DateTimeOffset(2030, 1, 9, 23, 0, 0, TimeSpan.Zero),
        userId = new { value = userId },
        location = new { entityName = "Venue", city = "Berlin", region = (string?)null, address = "Main St 1", country = "DE", longitude = 13.405, latitude = 52.52 },
        @event = new { eventType = (int)EventType.HouseParty, entrancePrice = (double?)null, currency = (int?)null, title, description, requirements = (string?)null, slots = 25, typeSpecificData = (string?)null },
    };
}
