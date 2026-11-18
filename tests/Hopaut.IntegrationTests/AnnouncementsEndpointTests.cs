using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Announcements.Infrastructure;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>
/// Plan Step 8.5 (D-033): the post's host or an administrator announces, edits and deletes; only the host, the post's
/// accepted attendees and administrators read (F-074); the inbox and the outbox list the caller's attended and hosted posts
/// that have announcements. Failure paths new in 8.5 answer problem details with an <c>errorCode</c>.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class AnnouncementsEndpointTests(HopautApiFactory factory)
{
    [Fact]
    public async Task GivenAnAnnouncement_WhenTheHostEditsIt_ThenTheTrimmedMessageIsReturnedAndInvalidOrForeignEditsAreRefused()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var post = await CreateOpenEventAsync(hostId, ct);
        using var host = ClientFor(hostId);
        using var stranger = ClientFor(NewUser("stranger"));
        using var created = await host.PostAsJsonAsync("/api/v1/announcements", CreateBody(post, " Doors open at eight "), ct);
        var location = created.Headers.Location!.ToString();

        // Act
        using var edited = await host.PutAsJsonAsync(location, new { message = "  Doors open at nine " }, ct);
        using var foreign = await stranger.PutAsJsonAsync(location, new { message = "Doors never open" }, ct);
        using var blank = await host.PutAsJsonAsync(location, new { message = "   " }, ct);
        using var missing = await host.PutAsJsonAsync("/api/v1/announcements/999999", new { message = "Doors open at nine" }, ct);
        using var tooLong = await host.PostAsJsonAsync("/api/v1/announcements", CreateBody(post, new string('x', 501)), ct);
        using var body = JsonDocument.Parse(await edited.Content.ReadAsStringAsync(ct));

        // Assert
        using (new AssertionScope())
        {
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            edited.StatusCode.Should().Be(HttpStatusCode.OK);
            location.Should().Be($"/api/v1/announcements/{body.RootElement.GetProperty("id").GetInt32()}");
            body.RootElement.GetProperty("postId").GetInt32().Should().Be(post.Value);
            body.RootElement.GetProperty("message").GetString().Should().Be("Doors open at nine");
            body.RootElement.GetProperty("editedAt").GetDateTimeOffset()
                .Should().BeOnOrAfter(body.RootElement.GetProperty("createdAt").GetDateTimeOffset());
            foreign.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await ErrorCodeAsync(foreign, ct)).Should().Be("announcements.announcement.not_post_host");
            blank.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await ValidationKeysAsync(blank, ct)).Should().Equal("message");
            missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await ErrorCodeAsync(missing, ct)).Should().Be("announcements.announcement.not_found");
            tooLong.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await ValidationKeysAsync(tooLong, ct)).Should().Equal("message");
        }
    }

    [Fact]
    public async Task GivenAPostsAnnouncements_WhenCallersReadThem_ThenOnlyTheHostItsAcceptedAttendeesAndAdministratorsMay()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var guestId = NewUser("guest");
        var post = await CreateOpenEventAsync(hostId, ct);
        await AttendAsync(guestId, post, ct);
        var first = await AnnounceAsync(hostId, post, "Doors open at eight", ct);
        var second = await AnnounceAsync(hostId, post, "Bring a towel", ct);
        using var guest = ClientFor(guestId);
        using var stranger = ClientFor(NewUser("stranger"));
        using var administrator = ClientFor(NewUser("admin"), HopautRoles.Admin);

        // Act
        using var firstPage = JsonDocument.Parse(await guest.GetStringAsync($"/api/v1/posts/{post.Value}/announcements?limit=1", ct));
        var cursor = firstPage.RootElement.GetProperty("nextCursor").GetString();
        using var secondPage = JsonDocument.Parse(await guest.GetStringAsync($"/api/v1/posts/{post.Value}/announcements?limit=1&cursor={cursor}", ct));
        using var list = JsonDocument.Parse(await guest.GetStringAsync($"/api/v1/announcements/post/{post.Value}", ct));
        using var forGuest = await guest.GetAsync($"/api/v1/announcements/{first.Value}", ct);
        using var forAdministrator = await administrator.GetAsync($"/api/v1/announcements/{first.Value}", ct);
        using var oneForStranger = await stranger.GetAsync($"/api/v1/announcements/{first.Value}", ct);
        using var listForStranger = await stranger.GetAsync($"/api/v1/announcements/post/{post.Value}", ct);
        using var pageForStranger = await stranger.GetAsync($"/api/v1/posts/{post.Value}/announcements", ct);
        using var missingPost = await guest.GetAsync("/api/v1/posts/999999/announcements", ct);
        using var invalidCursor = await guest.GetAsync($"/api/v1/posts/{post.Value}/announcements?cursor=not-a-cursor", ct);

        // Assert
        using (new AssertionScope())
        {
            Ids(firstPage.RootElement.GetProperty("items")).Should().Equal(second.Value);
            Ids(secondPage.RootElement.GetProperty("items")).Should().Equal(first.Value);
            secondPage.RootElement.GetProperty("nextCursor").ValueKind.Should().Be(JsonValueKind.Null);
            Ids(list.RootElement).Should().Equal(second.Value, first.Value);
            list.RootElement[1].GetProperty("editedAt").ValueKind.Should().Be(JsonValueKind.Null);
            forGuest.StatusCode.Should().Be(HttpStatusCode.OK);
            forAdministrator.StatusCode.Should().Be(HttpStatusCode.OK);
            foreach (var refused in new[] { oneForStranger, listForStranger, pageForStranger })
            {
                refused.StatusCode.Should().Be(HttpStatusCode.Forbidden);
                (await ErrorCodeAsync(refused, ct)).Should().Be("announcements.announcement.not_post_audience");
            }

            missingPost.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await ErrorCodeAsync(missingPost, ct)).Should().Be("announcements.announcement.post_not_found");
            invalidCursor.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await ValidationKeysAsync(invalidCursor, ct)).Should().Equal("cursor");
        }
    }

    [Fact]
    public async Task GivenAnnouncementsOnAttendedAndHostedPosts_WhenTheThreadsAreListed_ThenEachBoxShowsItsPostsWithTheirNewestAnnouncement()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var otherHostId = NewUser("other-host");
        var guestId = NewUser("guest");
        var announced = await CreateOpenEventAsync(hostId, ct);
        var silent = await CreateOpenEventAsync(hostId, ct);
        var elsewhere = await CreateOpenEventAsync(otherHostId, ct);
        await AttendAsync(guestId, announced, ct);
        await AttendAsync(guestId, silent, ct);
        await AttendAsync(guestId, elsewhere, ct);
        await AnnounceAsync(hostId, announced, "Doors open at eight", ct);
        await AnnounceAsync(otherHostId, elsewhere, "Parking is free", ct);
        var newest = await AnnounceAsync(hostId, announced, "Bring a towel", ct);
        using var guest = ClientFor(guestId);
        using var host = ClientFor(hostId);

        // Act
        using var inbox = JsonDocument.Parse(await guest.GetStringAsync("/api/v1/users/me/announcement-threads?limit=1", ct));
        var cursor = inbox.RootElement.GetProperty("nextCursor").GetString();
        using var inboxRest = JsonDocument.Parse(await guest.GetStringAsync($"/api/v1/users/me/announcement-threads?box=inbox&limit=1&cursor={cursor}", ct));
        using var outbox = JsonDocument.Parse(await host.GetStringAsync("/api/v1/users/me/announcement-threads?box=outbox", ct));
        using var unknown = await guest.GetAsync("/api/v1/users/me/announcement-threads?box=sent", ct);

        // Assert
        var thread = inbox.RootElement.GetProperty("items")[0];
        using (new AssertionScope())
        {
            thread.GetProperty("postId").GetInt32().Should().Be(announced.Value);
            thread.GetProperty("lastAnnouncement").GetProperty("id").GetInt32().Should().Be(newest.Value);
            thread.GetProperty("lastAnnouncement").GetProperty("message").GetString().Should().Be("Bring a towel");
            ThreadPostIds(inboxRest).Should().Equal(elsewhere.Value);
            inboxRest.RootElement.GetProperty("nextCursor").ValueKind.Should().Be(JsonValueKind.Null);
            ThreadPostIds(outbox).Should().Equal(announced.Value);
            unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await ValidationKeysAsync(unknown, ct)).Should().Equal("box");
        }
    }

    [Fact]
    public async Task GivenTwoEditsOfOneAnnouncement_WhenTheSecondIsSavedAfterTheFirst_ThenItIsAConflict()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var post = await CreateOpenEventAsync(hostId, ct);
        var id = await AnnounceAsync(hostId, post, "Doors open at eight", ct);
        await using var first = factory.Services.CreateAsyncScope();
        await using var second = factory.Services.CreateAsyncScope();
        var byFirst = await first.ServiceProvider.GetRequiredService<IAnnouncementRepository>().GetByIdAsync(id, ct);
        var bySecond = await second.ServiceProvider.GetRequiredService<IAnnouncementRepository>().GetByIdAsync(id, ct);
        byFirst!.Edit(AnnouncementMessage.Create("Doors open at nine").Value, DateTimeOffset.UtcNow);
        await first.ServiceProvider.GetRequiredService<IAnnouncementsUnitOfWork>().SaveChangesAsync(ct);
        bySecond!.Edit(AnnouncementMessage.Create("Doors open at ten").Value, DateTimeOffset.UtcNow);

        // Act
        var act = () => second.ServiceProvider.GetRequiredService<IAnnouncementsUnitOfWork>().SaveChangesAsync(ct);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyConflictException>();
        (await MessageAsync(id, ct)).Should().Be("Doors open at nine");
    }

    private static string NewUser(string role) => $"announcements-{role}-{Guid.NewGuid():N}";

    // Today's create body, with the strongly-typed post ID as { "value": … } (D-016).
    private static object CreateBody(PostId post, string message) => new { postId = new { value = post.Value }, message };

    private static List<int> Ids(JsonElement announcements) =>
        announcements.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();

    private static List<int> ThreadPostIds(JsonDocument page) =>
        page.RootElement.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("postId").GetInt32()).ToList();

    private HttpClient ClientFor(string userId, params string[] roles)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(userId, roles));
        return client;
    }

    private async Task<PostId> CreateOpenEventAsync(string hostId, CancellationToken ct)
    {
        var created = await TestCaller.SendAsync(factory.Services, caller: null, new CreatePostCommand(
            new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2030, 1, 9, 23, 0, 0, TimeSpan.Zero),
            UserId.From(hostId),
            new CreatePostLocationInput("Venue", "Berlin", null, "Main St 1", "DE", 13.405, 52.52),
            new CreatePostEventInput(EventType.Bar, null, null, "Announcements", "announcements endpoint tests", null, null, null)), ct);
        return created.Value;
    }

    // Events other than house parties accept attendance requests at once (D-030).
    private async Task AttendAsync(string guestId, PostId post, CancellationToken ct)
    {
        var requested = await TestCaller.SendAsync(factory.Services, TestCaller.For(guestId), new RequestAttendanceCommand(post), ct);
        requested.IsSuccess.Should().BeTrue();
    }

    private async Task<AnnouncementId> AnnounceAsync(string hostId, PostId post, string message, CancellationToken ct)
    {
        var created = await TestCaller.SendAsync(factory.Services, TestCaller.For(hostId), new CreateAnnouncementCommand(post, message), ct);
        return created.Value;
    }

    private async Task<string> MessageAsync(AnnouncementId id, CancellationToken ct)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var announcement = await scope.ServiceProvider.GetRequiredService<AnnouncementsModuleDbContext>().Announcements
            .AsNoTracking()
            .SingleAsync(a => a.Id == id, ct);
        return announcement.Message.Value;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return problem.RootElement.TryGetProperty("errorCode", out var code) ? code.GetString() : null;
    }

    private static async Task<List<string>> ValidationKeysAsync(HttpResponseMessage response, CancellationToken ct)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return problem.RootElement.GetProperty("errors").EnumerateObject().Select(error => error.Name).ToList();
    }
}
