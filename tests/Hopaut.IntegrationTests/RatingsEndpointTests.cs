using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Ratings.Contracts;
using Hopaut.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>
/// Plan Step 8.4 (D-031): accepted attendees rate the host of a post once; the rater is the caller and the rated user the
/// host, and the reputation projection changes in the same save as the rating.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class RatingsEndpointTests(HopautApiFactory factory)
{
    [Fact]
    public async Task GivenAnAcceptedAttendee_WhenTheyRateThePost_ThenTheHostIsRatedOnceAndTheReadsAndReputationShowIt()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var guestId = NewUser("guest");
        var post = await CreateOpenEventAsync(hostId, ct);
        await TestCaller.SendAsync(factory.Services, TestCaller.For(guestId), new RequestAttendanceCommand(post), ct);
        using var guest = ClientFor(guestId);

        // Act
        using var created = await guest.PostAsJsonAsync("/api/v1/ratings", Body(post, rate: 5, feedback: " Great host "), ct);
        using var duplicate = await guest.PostAsJsonAsync("/api/v1/ratings", Body(post, rate: 1, feedback: null), ct);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var ratingId = createdBody.RootElement.GetProperty("id").GetInt32();
        using var rating = JsonDocument.Parse(await guest.GetStringAsync($"/api/v1/ratings/{ratingId}", ct));
        using var received = JsonDocument.Parse(await guest.GetStringAsync($"/api/v1/ratings/user/{hostId}", ct));
        var reputation = await ReputationAsync(hostId, ct);

        // Assert
        using (new AssertionScope())
        {
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            created.Headers.Location!.ToString().Should().Be($"/api/v1/ratings/{ratingId}");
            duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
            (await ErrorCodeAsync(duplicate, ct)).Should().Be("ratings.rating.already_rated");
            rating.RootElement.GetProperty("rate").GetInt32().Should().Be(5);
            rating.RootElement.GetProperty("raterId").GetString().Should().Be(guestId);
            rating.RootElement.GetProperty("feedback").GetString().Should().Be("Great host");
            received.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).Should().Equal(ratingId);
            reputation.Should().Be(new UserReputationSummary(UserId.From(hostId), 1, 5.0));
        }
    }

    [Fact]
    public async Task GivenCallersWhoMayNotRate_WhenTheyRate_ThenEachRefusalIsAProblemWithItsCode()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var post = await CreateOpenEventAsync(hostId, ct);
        using var stranger = ClientFor(NewUser("stranger"));
        using var host = ClientFor(hostId);

        // Act
        using var notAttendee = await stranger.PostAsJsonAsync("/api/v1/ratings", Body(post, rate: 4, feedback: null), ct);
        using var missingPost = await stranger.PostAsJsonAsync("/api/v1/ratings", Body(PostId.From(999_999), rate: 4, feedback: null), ct);
        using var missingRating = await host.GetAsync("/api/v1/ratings/999999", ct);

        // Assert
        using (new AssertionScope())
        {
            notAttendee.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await ErrorCodeAsync(notAttendee, ct)).Should().Be("ratings.rating.not_attendee");
            missingPost.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await ErrorCodeAsync(missingPost, ct)).Should().Be("ratings.rating.post_not_found");
            missingRating.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await ErrorCodeAsync(missingRating, ct)).Should().Be("ratings.rating.not_found");
        }
    }

    private static string NewUser(string role) => $"ratings-{role}-{Guid.NewGuid():N}";

    // Today's request shape, with strongly-typed IDs as { "value": … } (D-016); rated and rater IDs in the body are ignored.
    private static object Body(PostId post, int rate, string? feedback) => new
    {
        rate,
        userId = new { value = "ignored-rated" },
        raterId = new { value = "ignored-rater" },
        postId = new { value = post.Value },
        feedback,
    };

    private HttpClient ClientFor(string userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(userId));
        return client;
    }

    private async Task<PostId> CreateOpenEventAsync(string hostId, CancellationToken ct)
    {
        var created = await TestCaller.SendAsync(factory.Services, caller: null, new CreatePostCommand(
            new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2030, 1, 9, 23, 0, 0, TimeSpan.Zero),
            UserId.From(hostId),
            new CreatePostLocationInput("Venue", "Berlin", null, "Main St 1", "DE", 13.405, 52.52),
            new CreatePostEventInput(EventType.Bar, null, null, "Ratings", "ratings endpoint tests", null, null, null)), ct);
        return created.Value;
    }

    private async Task<UserReputationSummary?> ReputationAsync(string userId, CancellationToken ct)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var reputations = await scope.ServiceProvider.GetRequiredService<IUserReputationQueries>().GetAsync([UserId.From(userId)], ct);
        return reputations.GetValueOrDefault(UserId.From(userId));
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return problem.RootElement.TryGetProperty("errorCode", out var code) ? code.GetString() : null;
    }
}
