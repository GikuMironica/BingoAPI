using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Infrastructure;
using Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Attendance.Infrastructure;
using Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.Modules.Moderation.Infrastructure;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Application.Commands.DeletePost;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Infrastructure;
using Hopaut.Modules.Ratings.Application.Commands.CreateRating;
using Hopaut.Modules.Ratings.Infrastructure;
using Hopaut.Modules.Users.Domain;
using Hopaut.Modules.Users.Infrastructure;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>
/// Plan Step 6.9: handlers authorize the loaded resource through IResourceAuthorizer against the policies each module
/// registers, and reports are reachable only through the Admin policy. Status codes are today's (D-021): endpoints that map
/// every failure to 404 keep doing so for callers who may not act.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class ResourceAuthorizationTests(HopautApiFactory factory)
{
    private const string Other = "other";
    private const string Owner = "owner";
    private const string Administrator = "administrator";
    private const string PictureUrl = "https://cdn.hopaut.app/profiles/authz.png";

    [Theory]
    [InlineData(Other, HttpStatusCode.NotFound, true)]
    [InlineData(Owner, HttpStatusCode.NoContent, false)]
    [InlineData(Administrator, HttpStatusCode.NoContent, false)]
    public async Task GivenAPost_WhenACallerDeletesIt_ThenOnlyItsHostOrAnAdministratorSucceeds(
        string caller, HttpStatusCode expected, bool remains)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var host = NewUser("host");
        var post = await CreatePostAsync(host, ct);
        using var client = ClientFor(caller, host);

        // Act
        using var response = await client.DeleteAsync($"/api/v1/posts/{post.Value}", ct);

        // Assert
        var exists = await QueryAsync<PostsModuleDbContext, bool>(db => db.Posts.AnyAsync(p => p.Id == post, ct));
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(expected);
            exists.Should().Be(remains);
        }
    }

    [Fact]
    public async Task GivenNoRequest_WhenAPostIsDeleted_ThenTheAnonymousCallerIsRefused()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var post = await CreatePostAsync(NewUser("host"), ct);

        // Act
        var result = await TestCaller.SendAsync(factory.Services, caller: null, new DeletePostCommand(post), ct);

        // Assert
        var exists = await QueryAsync<PostsModuleDbContext, bool>(db => db.Posts.AnyAsync(p => p.Id == post, ct));
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.not_host");
            exists.Should().BeTrue();
        }
    }

    [Fact]
    public async Task GivenAnotherUsersPost_WhenTheCallerAnnouncesOnIt_ThenItIsForbidden()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var post = await CreatePostAsync(NewUser("host"), ct);
        using var client = ClientFor(Other, ownerId: "unused");

        // Act
        using var response = await client.PostAsJsonAsync("/api/v1/announcements", AnnouncementBody(post), ct);

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await ErrorCodeAsync(response, ct)).Should().Be("announcements.announcement.not_post_host");
        }
    }

    [Theory]
    [InlineData(Owner)]
    [InlineData(Administrator)]
    public async Task GivenThePostsHostOrAnAdministrator_WhenAnnouncingOnThePost_ThenTheAnnouncementIsCreated(string caller)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var host = NewUser("host");
        var post = await CreatePostAsync(host, ct);
        using var client = ClientFor(caller, host);

        // Act
        using var response = await client.PostAsJsonAsync("/api/v1/announcements", AnnouncementBody(post), ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GivenAMissingPost_WhenAnAnnouncementIsCreated_ThenItIsNotFound()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var client = ClientFor(Other, ownerId: "unused");

        // Act
        using var response = await client.PostAsJsonAsync("/api/v1/announcements", AnnouncementBody(PostId.From(int.MaxValue)), ct);

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await ErrorCodeAsync(response, ct)).Should().Be("announcements.announcement.post_not_found");
        }
    }

    [Theory]
    [InlineData(Other, HttpStatusCode.NotFound, true)]
    [InlineData(Owner, HttpStatusCode.NoContent, false)]
    [InlineData(Administrator, HttpStatusCode.NoContent, false)]
    public async Task GivenAnAnnouncement_WhenACallerDeletesIt_ThenOnlyThePostsHostOrAnAdministratorSucceeds(
        string caller, HttpStatusCode expected, bool remains)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var host = NewUser("host");
        var post = await CreatePostAsync(host, ct);
        var announcement = await TestCaller.SendAsync(
            factory.Services, TestCaller.For(host), new CreateAnnouncementCommand(post, "Doors open at eight"), ct);
        using var client = ClientFor(caller, host);

        // Act
        using var response = await client.DeleteAsync($"/api/v1/announcements/{announcement.Value.Value}", ct);

        // Assert
        var exists = await QueryAsync<AnnouncementsModuleDbContext, bool>(
            db => db.Announcements.AnyAsync(a => a.Id == announcement.Value, ct));
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(expected);
            exists.Should().Be(remains);
        }
    }

    [Theory]
    [InlineData("accept", Other)]
    [InlineData("reject", Other)]
    [InlineData("accept", Administrator)]
    [InlineData("reject", Administrator)]
    public async Task GivenAnAttendanceRequest_WhenSomeoneOtherThanTheHostDecidesIt_ThenItIsNotFoundAndStaysPending(
        string decision, string caller)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var host = NewUser("host");
        var attendee = NewUser("attendee");
        var post = await CreatePostAsync(host, ct);
        await RequestAttendanceAsync(post, attendee, ct);
        using var client = ClientFor(caller, host);

        // Act
        using var response = await client.PostAsync($"/api/v1/attendance/{post.Value}/{decision}/{attendee}", content: null, ct);

        // Assert
        var status = await AttendanceStatusAsync(post, attendee, ct);
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            status.Should().Be(AttendanceStatus.Pending);
        }
    }

    [Theory]
    [InlineData("accept", AttendanceStatus.Accepted)]
    [InlineData("reject", AttendanceStatus.Rejected)]
    public async Task GivenAnAttendanceRequest_WhenTheHostDecidesIt_ThenTheDecisionIsSaved(string decision, AttendanceStatus expected)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var host = NewUser("host");
        var attendee = NewUser("attendee");
        var post = await CreatePostAsync(host, ct);
        await RequestAttendanceAsync(post, attendee, ct);
        using var client = ClientFor(Owner, host);

        // Act
        using var response = await client.PostAsync($"/api/v1/attendance/{post.Value}/{decision}/{attendee}", content: null, ct);

        // Assert
        var status = await AttendanceStatusAsync(post, attendee, ct);
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            status.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(Other, HttpStatusCode.NotFound, true)]
    [InlineData(Owner, HttpStatusCode.NoContent, false)]
    [InlineData(Administrator, HttpStatusCode.NoContent, false)]
    public async Task GivenARating_WhenACallerDeletesIt_ThenOnlyItsAuthorOrAnAdministratorSucceeds(
        string caller, HttpStatusCode expected, bool remains)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var rater = NewUser("rater");
        var post = await CreatePostAsync(NewUser("rated"), ct, EventType.Bar);
        await TestCaller.SendAsync(factory.Services, TestCaller.For(rater), new RequestAttendanceCommand(post), ct);
        var rating = await TestCaller.SendAsync(factory.Services, TestCaller.For(rater), new CreateRatingCommand(4, post, null), ct);
        using var client = ClientFor(caller, rater);

        // Act
        using var response = await client.DeleteAsync($"/api/v1/ratings/{rating.Value.Value}", ct);

        // Assert
        var exists = await QueryAsync<RatingsModuleDbContext, bool>(db => db.Ratings.AnyAsync(r => r.Id == rating.Value, ct));
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(expected);
            exists.Should().Be(remains);
        }
    }

    [Theory]
    [InlineData("profile", Other, HttpStatusCode.NotFound)]
    [InlineData("profile", Owner, HttpStatusCode.NoContent)]
    [InlineData("profile", Administrator, HttpStatusCode.NoContent)]
    [InlineData("profile-picture", Other, HttpStatusCode.NotFound)]
    [InlineData("profile-picture", Owner, HttpStatusCode.NoContent)]
    [InlineData("profile-picture", Administrator, HttpStatusCode.NoContent)]
    public async Task GivenAProfile_WhenACallerUpdatesIt_ThenOnlyItsOwnerOrAnAdministratorSucceeds(
        string route, string caller, HttpStatusCode expected)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var owner = NewUser("profile");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UsersModuleDbContext>();
            db.UserProfiles.Add(UserProfile.Create(owner, "Ada", "Lovelace", 0, new DateTimeOffset(2024, 1, 9, 0, 0, 0, TimeSpan.Zero)));
            await db.SaveChangesAsync(ct);
        }

        using var client = ClientFor(caller, owner);
        object body = route == "profile"
            ? new { firstName = "Grace", lastName = "Hopper", description = "Compiler pioneer" }
            : new { pictureUrl = PictureUrl };

        // Act
        using var response = await client.PutAsJsonAsync($"/api/v1/users/{owner}/{route}", body, ct);

        // Assert
        var profile = await QueryAsync<UsersModuleDbContext, UserProfile>(
            db => db.UserProfiles.AsNoTracking().SingleAsync(p => p.IdentityUserId == owner, ct));
        var changed = route == "profile" ? profile.FirstName == "Grace" : profile.ProfilePicture == PictureUrl;
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(expected);
            changed.Should().Be(expected == HttpStatusCode.NoContent);
        }
    }

    [Fact]
    public async Task GivenACallerWithoutTheAdminRole_WhenReportsAreListedOrResolved_ThenItIsForbidden()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var report = await CreatePostReportAsync(ct);
        using var client = ClientFor(Other, ownerId: "unused");

        // Act
        using var list = await client.GetAsync("/api/v1/moderation/reports/post/open", ct);
        using var resolve = await client.PostAsync($"/api/v1/moderation/reports/post/{report.Value}/resolve", content: null, ct);

        // Assert
        var status = await ReportStatusAsync(report, ct);
        using (new AssertionScope())
        {
            list.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            resolve.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            status.Should().Be(ReportStatus.Open);
        }
    }

    [Fact]
    public async Task GivenAnAdministrator_WhenOpenReportsAreListedAndOneIsResolved_ThenBothSucceed()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var report = await CreatePostReportAsync(ct);
        using var client = ClientFor(Administrator, ownerId: "unused");

        // Act
        using var list = await client.GetAsync("/api/v1/moderation/reports/post/open", ct);
        using var resolve = await client.PostAsync($"/api/v1/moderation/reports/post/{report.Value}/resolve", content: null, ct);

        // Assert
        var status = await ReportStatusAsync(report, ct);
        using (new AssertionScope())
        {
            list.StatusCode.Should().Be(HttpStatusCode.OK);
            resolve.StatusCode.Should().Be(HttpStatusCode.OK);
            status.Should().Be(ReportStatus.Resolved);
        }
    }

    private static string NewUser(string role) => $"authz-{role}-{Guid.NewGuid():N}";

    /// <summary>A client whose bearer token belongs to the resource owner, an administrator or an unrelated user.</summary>
    private HttpClient ClientFor(string caller, string ownerId)
    {
        var (userId, roles) = caller switch
        {
            Owner => (ownerId, Array.Empty<string>()),
            Administrator => (NewUser("admin"), [HopautRoles.Admin]),
            Other => (NewUser("other"), [HopautRoles.User]),
            _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null),
        };

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(userId, roles));
        return client;
    }

    // Strongly-typed IDs are bound as { "value": n } until the wire switches to primitives (D-016, plan Step 10.1).
    private static object AnnouncementBody(PostId post) => new { postId = new { value = post.Value }, message = "Doors open at eight" };

    private async Task<PostId> CreatePostAsync(string hostId, CancellationToken ct, EventType type = EventType.HouseParty)
    {
        var created = await TestCaller.SendAsync(factory.Services, caller: null, new CreatePostCommand(
            new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2030, 1, 9, 23, 0, 0, TimeSpan.Zero),
            UserId.From(hostId),
            new CreatePostLocationInput("Venue", "Berlin", null, "Main St 1", "DE", 13.405, 52.52),
            new CreatePostEventInput(type, null, null, "Authorization", "authorization tests", null, 10, null)), ct);
        return created.Value;
    }

    private async Task RequestAttendanceAsync(PostId post, string attendee, CancellationToken ct)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(attendee));
        using var response = await client.PostAsync($"/api/v1/attendance/{post.Value}/request", content: null, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task<PostReportId> CreatePostReportAsync(CancellationToken ct)
    {
        var created = await TestCaller.SendAsync(
            factory.Services,
            caller: null,
            new CreatePostReportCommand(PostId.From(1), UserId.From(NewUser("reporter")), UserId.From(NewUser("host")), 1, null),
            ct);
        return created.Value;
    }

    private Task<AttendanceStatus> AttendanceStatusAsync(PostId post, string attendee, CancellationToken ct) =>
        QueryAsync<AttendanceModuleDbContext, AttendanceStatus>(db => db.Participations
            .Where(p => p.PostId == post && p.UserId == UserId.From(attendee))
            .Select(p => p.Status)
            .SingleAsync(ct));

    private Task<ReportStatus> ReportStatusAsync(PostReportId report, CancellationToken ct) =>
        QueryAsync<ModerationModuleDbContext, ReportStatus>(db => db.PostReports
            .Where(r => r.Id == report)
            .Select(r => r.Status)
            .SingleAsync(ct));

    private async Task<TResult> QueryAsync<TContext, TResult>(Func<TContext, Task<TResult>> query)
        where TContext : DbContext
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<TContext>());
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return problem.RootElement.TryGetProperty("errorCode", out var code) ? code.GetString() : null;
    }
}
