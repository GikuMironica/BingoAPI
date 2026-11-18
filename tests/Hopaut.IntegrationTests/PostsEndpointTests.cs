using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Posts.Application.Queries;
using Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Infrastructure;
using Hopaut.SharedKernel;
using Mediator;
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

    [Fact]
    public async Task GivenAnEntrancePriceAndLegacyCurrency_WhenThePostIsCreatedAndRead_ThenTodaysFieldsRoundTrip()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create("posts-fee"));
        using var created = await client.PostAsJsonAsync("/api/v1/posts", Body(userId: "posts-fee", title: "Paid party", entrancePrice: 12.5, currency: 2), ct);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var postId = createdBody.RootElement.GetProperty("postId").GetInt32();

        // Act
        using var post = JsonDocument.Parse(await client.GetStringAsync($"/api/v1/posts/{postId}", ct));

        // Assert
        var @event = post.RootElement.GetProperty("event");
        using (new AssertionScope())
        {
            @event.GetProperty("entrancePrice").GetDouble().Should().Be(12.5);
            @event.GetProperty("currency").GetInt32().Should().Be(2);
            post.RootElement.GetProperty("activeFlag").GetInt32().Should().Be(1);
        }
    }
    [Fact]
    public async Task GivenTagNames_WhenTwoPostsAreCreatedAndOneIsRead_ThenTagsAreNormalisedSavedOnceAndReturned()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create("posts-tags"));
        var unique = "t" + Guid.NewGuid().ToString("N")[..12];

        // Act
        var first = await CreateAsync(client, Body(userId: "posts-tags", title: "Tagged", tagNames: [unique.ToUpperInvariant(), " Jazz ", "jazz"]), ct);
        await CreateAsync(client, Body(userId: "posts-tags", title: "Tagged too", tagNames: [unique]), ct);
        using var post = JsonDocument.Parse(await client.GetStringAsync($"/api/v1/posts/{first}", ct));

        // Assert
        await using var scope = factory.Services.CreateAsyncScope();
        var stored = await scope.ServiceProvider.GetRequiredService<PostsModuleDbContext>().Tags
            .CountAsync(tag => tag.Name == TagName.Create(unique).Value, ct);
        using (new AssertionScope())
        {
            post.RootElement.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()).Should().BeEquivalentTo(unique, "jazz");
            stored.Should().Be(1);
        }
    }

    [Fact]
    public async Task GivenTooManyTags_WhenAPostIsCreated_ThenTheTagNamesFieldIsReported()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create("posts-many-tags"));
        string[] tags = [.. Enumerable.Range(1, Post.MaxTags + 1).Select(i => $"many{i}")];

        // Act
        using var response = await client.PostAsJsonAsync("/api/v1/posts", Body(userId: "posts-many-tags", title: "Over tagged", tagNames: tags), ct);

        // Assert
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            problem.RootElement.GetProperty("errors").EnumerateObject().Select(e => e.Name).Should().Equal("tagNames");
        }
    }

    [Fact]
    public async Task GivenTheHost_WhenThePostIsPatched_ThenTheChangesAreSavedAndReturned()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = ClientFor("posts-patch-host");
        var postId = await CreateAsync(client, Body(userId: "posts-patch-host", title: "Before"), ct);

        // Act
        using var response = await client.PatchAsJsonAsync(
            $"/api/v1/posts/{postId}",
            new
            {
                location = new { city = "Munich", longitude = 11.575, latitude = 48.137 },
                @event = new { title = "After", slots = 30, entrancePrice = 7.5, currency = 1 },
                tagNames = new[] { "Patched" },
            },
            ct);

        // Assert
        using var post = JsonDocument.Parse(await client.GetStringAsync($"/api/v1/posts/{postId}", ct));
        var @event = post.RootElement.GetProperty("event");
        var location = post.RootElement.GetProperty("location");
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            @event.GetProperty("title").GetString().Should().Be("After");
            @event.GetProperty("slots").GetInt32().Should().Be(30);
            @event.GetProperty("entrancePrice").GetDouble().Should().Be(7.5);
            @event.GetProperty("currency").GetInt32().Should().Be(1);
            location.GetProperty("city").GetString().Should().Be("Munich");
            location.GetProperty("latitude").GetDouble().Should().Be(48.137);
            post.RootElement.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()).Should().Equal("patched");
        }
    }

    [Fact]
    public async Task GivenAnotherUser_WhenThePostIsPatched_ThenItIsForbiddenAndUnchanged()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var host = ClientFor("posts-patch-owner");
        using var stranger = ClientFor("posts-patch-stranger");
        var postId = await CreateAsync(host, Body(userId: "posts-patch-owner", title: "Mine"), ct);

        // Act
        using var response = await stranger.PatchAsJsonAsync($"/api/v1/posts/{postId}", new { @event = new { title = "Hijacked" } }, ct);

        // Assert
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        using var post = JsonDocument.Parse(await host.GetStringAsync($"/api/v1/posts/{postId}", ct));
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            problem.RootElement.GetProperty("errorCode").GetString().Should().Be("posts.post.not_host");
            post.RootElement.GetProperty("event").GetProperty("title").GetString().Should().Be("Mine");
        }
    }

    [Fact]
    public async Task GivenAnInvalidEditOrAMissingPost_WhenPatched_ThenTheProblemsAreValidationAndNotFound()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = ClientFor("posts-patch-invalid");
        var postId = await CreateAsync(client, Body(userId: "posts-patch-invalid", title: "Valid"), ct);

        // Act
        using var invalid = await client.PatchAsJsonAsync($"/api/v1/posts/{postId}", new { @event = new { title = new string('t', 31) } }, ct);
        using var missing = await client.PatchAsJsonAsync("/api/v1/posts/999999", new { @event = new { title = "Ghost" } }, ct);

        // Assert
        using var validation = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync(ct));
        using var notFound = JsonDocument.Parse(await missing.Content.ReadAsStringAsync(ct));
        using (new AssertionScope())
        {
            invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validation.RootElement.GetProperty("errors").EnumerateObject().Select(e => e.Name).Should().Equal("event.title");
            missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
            notFound.RootElement.GetProperty("errorCode").GetString().Should().Be("posts.post.not_found");
        }
    }

    [Fact]
    public async Task GivenActiveEndedAndDisabledPosts_WhenTheHostListsThem_ThenPagesAreNewestFirstAndScopedToTheCaller()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = "posts-mine-" + Guid.NewGuid().ToString("N");
        var otherId = "posts-other-" + Guid.NewGuid().ToString("N");
        using var host = ClientFor(hostId);
        using var other = ClientFor(otherId);
        using var admin = ClientFor("posts-admin", HopautRoles.Admin);
        var start = new DateTimeOffset(2031, 3, 1, 18, 0, 0, TimeSpan.Zero);
        var soon = await CreateAsync(host, Body(hostId, "Soon", start: start), ct);
        var later = await CreateAsync(host, Body(hostId, "Later", start: start.AddDays(1)), ct);
        var latest = await CreateAsync(host, Body(hostId, "Latest", start: start.AddDays(2)), ct);
        var disabled = await CreateAsync(host, Body(hostId, "Disabled", start: start.AddDays(3)), ct);
        var justStarted = DateTimeOffset.UtcNow.AddMinutes(-29);
        var ended = await CreateAsync(host, Body(hostId, "Ended", start: justStarted, end: justStarted.AddMinutes(15)), ct);
        await CreateAsync(other, Body(otherId, "Not mine", start: justStarted, end: justStarted.AddMinutes(15)), ct);
        using var disabling = await admin.PatchAsJsonAsync($"/api/v1/posts/{disabled}/status", new { status = "disabled" }, ct);

        // Act
        var firstPage = await GetPageAsync(host, "/api/v1/users/me/posts?limit=2", ct);
        var secondPage = await GetPageAsync(host, $"/api/v1/users/me/posts?limit=2&cursor={firstPage.NextCursor}", ct);
        var inactive = await GetPageAsync(host, "/api/v1/users/me/posts?status=inactive", ct);

        // Assert
        using (new AssertionScope())
        {
            disabling.StatusCode.Should().Be(HttpStatusCode.NoContent);
            firstPage.Ids.Should().Equal(latest, later);
            firstPage.NextCursor.Should().NotBeNull();
            secondPage.Ids.Should().Equal(soon);
            secondPage.NextCursor.Should().BeNull();
            inactive.Ids.Should().Equal(disabled, ended);
        }
    }

    [Fact]
    public async Task GivenAPost_WhenItsStatusIsPatched_ThenOnlyAdministratorsDisableItAndOnlyOnce()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var host = ClientFor("posts-status-host");
        using var admin = ClientFor("posts-status-admin", HopautRoles.Admin);
        var postId = await CreateAsync(host, Body("posts-status-host", "Moderated"), ct);

        // Act
        using var byHost = await host.PatchAsJsonAsync($"/api/v1/posts/{postId}/status", new { status = "disabled" }, ct);
        using var unknown = await admin.PatchAsJsonAsync($"/api/v1/posts/{postId}/status", new { status = "active" }, ct);
        using var first = await admin.PatchAsJsonAsync($"/api/v1/posts/{postId}/status", new { status = "disabled" }, ct);
        using var second = await admin.PatchAsJsonAsync($"/api/v1/posts/{postId}/status", new { status = "disabled" }, ct);

        // Assert
        using var conflict = JsonDocument.Parse(await second.Content.ReadAsStringAsync(ct));
        using (new AssertionScope())
        {
            byHost.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            first.StatusCode.Should().Be(HttpStatusCode.NoContent);
            second.StatusCode.Should().Be(HttpStatusCode.Conflict);
            conflict.RootElement.GetProperty("errorCode").GetString().Should().Be("posts.post.already_disabled");
        }
    }

    [Fact]
    public async Task GivenPostsAroundAPoint_WhenTheFeedIsRead_ThenOnlyListedRunningPostsInsideTheRadiusComeNearestFirst()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = "posts-feed-" + Guid.NewGuid().ToString("N");
        using var host = ClientFor(hostId);
        using var admin = ClientFor("posts-feed-admin", HopautRoles.Admin);
        var (longitude, latitude) = (151.21, -33.86); // far from every other test's posts
        var center = await CreateAsync(host, Body(hostId, "Center", longitude: longitude, latitude: latitude), ct);
        var near = await CreateAsync(host, Body(hostId, "Near", longitude: longitude, latitude: latitude + 0.0045), ct);
        var far = await CreateAsync(host, Body(hostId, "Two km", longitude: longitude, latitude: latitude + 0.018), ct);
        await CreateAsync(host, Body(hostId, "Outside", longitude: longitude, latitude: latitude + 0.18), ct);
        var justStarted = DateTimeOffset.UtcNow.AddMinutes(-29);
        await CreateAsync(host, Body(hostId, "Ended", start: justStarted, end: justStarted.AddMinutes(15), longitude: longitude, latitude: latitude), ct);
        var disabled = await CreateAsync(host, Body(hostId, "Disabled", longitude: longitude, latitude: latitude), ct);
        using var disabling = await admin.PatchAsJsonAsync($"/api/v1/posts/{disabled}/status", new { status = "disabled" }, ct);
        var feed = FormattableString.Invariant($"/api/v1/posts/nearby?longitude={longitude}&latitude={latitude}");

        // Act
        var all = await GetIdsAsync(host, feed + "&radius=5000", ct);
        var firstTwo = await GetIdsAsync(host, feed + "&radius=5000&limit=2", ct);
        using var tooWide = await host.GetAsync(feed + "&radius=15001", ct);
        using var tooMany = await host.GetAsync(feed + "&limit=101", ct);
        var firstPage = await SendAsync(new GetNearbyPostsQuery(longitude, latitude, 5000, Limit: 2), ct);
        var secondPage = await SendAsync(new GetNearbyPostsQuery(longitude, latitude, 5000, Limit: 2, Cursor: firstPage.NextCursor), ct);

        // Assert
        using (new AssertionScope())
        {
            all.Should().Equal(center, near, far);
            firstTwo.Should().Equal(center, near);
            tooWide.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            tooMany.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            firstPage.Items.Select(post => post.Id).Should().Equal(center, near);
            secondPage.Items.Select(post => post.Id).Should().Equal(far);
            secondPage.NextCursor.Should().BeNull();
        }
    }

    private async Task<PostPage> SendAsync(GetNearbyPostsQuery query, CancellationToken ct)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ISender>().Send(query, ct);
        return result.Value;
    }

    private static async Task<List<int>> GetIdsAsync(HttpClient client, string url, CancellationToken ct)
    {
        using var response = await client.GetAsync(url, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var posts = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return posts.RootElement.EnumerateArray().Select(post => post.GetProperty("id").GetInt32()).ToList();
    }

    private static async Task<(List<int> Ids, string? NextCursor)> GetPageAsync(HttpClient client, string url, CancellationToken ct)
    {
        using var response = await client.GetAsync(url, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var page = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var ids = page.RootElement.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();
        var next = page.RootElement.GetProperty("nextCursor");
        return (ids, next.ValueKind == JsonValueKind.Null ? null : next.GetString());
    }

    private HttpClient ClientFor(string userId, params string[] roles)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(userId, roles));
        return client;
    }

    private static async Task<int> CreateAsync(HttpClient client, object body, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/posts", body, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var created = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return created.RootElement.GetProperty("postId").GetInt32();
    }

    // Today's request shape: the command itself, with strongly-typed IDs as { "value": … } (D-016, plan Step 10.4).
    private static object Body(
        string userId,
        string title,
        string description = "Bring snacks and good vibes",
        double? entrancePrice = null,
        int? currency = null,
        string[]? tagNames = null,
        DateTimeOffset? start = null,
        DateTimeOffset? end = null,
        double longitude = 13.405,
        double latitude = 52.52) => new
        {
            eventTime = start ?? new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero),
            endTime = end ?? (start ?? new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero)).AddHours(5),
            userId = new { value = userId },
            location = new { entityName = "Venue", city = "Berlin", region = (string?)null, address = "Main St 1", country = "DE", longitude, latitude },
            @event = new { eventType = (int)EventType.HouseParty, entrancePrice, currency, title, description, requirements = (string?)null, slots = 25, typeSpecificData = (string?)null },
            tagNames,
        };
}
