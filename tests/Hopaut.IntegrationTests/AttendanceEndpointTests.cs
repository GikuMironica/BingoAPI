using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Attendance.Infrastructure;
using Hopaut.Modules.Posts.Application.Commands.CreatePost;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>
/// Plan Step 8.3 (D-030): attendance follows the legacy rules, the existing failure paths keep today's responses (409 text,
/// empty 404) and the new ones answer problem details with an <c>errorCode</c>.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class AttendanceEndpointTests(HopautApiFactory factory)
{
    [Fact]
    public async Task GivenAnOpenEvent_WhenAGuestRequestsToAttendTwice_ThenTheyAttendAtOnceAndTheSecondRequestIsAlreadyRequested()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var post = await CreatePostAsync(NewUser("host"), EventType.Bar, slots: null, ct);
        using var guest = ClientFor(NewUser("guest"));

        // Act
        using var first = await guest.PostAsync($"/api/v1/attendance/{post.Value}/request", content: null, ct);
        using var second = await guest.PostAsync($"/api/v1/attendance/{post.Value}/request", content: null, ct);
        using var check = JsonDocument.Parse(await guest.GetStringAsync($"/api/v1/attendance/{post.Value}/check", ct));

        // Assert
        using (new AssertionScope())
        {
            first.StatusCode.Should().Be(HttpStatusCode.OK);
            second.StatusCode.Should().Be(HttpStatusCode.Conflict);
            (await second.Content.ReadAsStringAsync(ct)).Should().Be("\"Already requested\"");
            check.RootElement.GetProperty("attending").GetBoolean().Should().BeTrue();
        }
    }

    [Fact]
    public async Task GivenPostsThatCannotBeAttended_WhenRequested_ThenEachRefusalIsAProblemWithItsCode()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var hosted = await CreatePostAsync(hostId, EventType.HouseParty, slots: null, ct);
        var justStarted = DateTimeOffset.UtcNow.AddMinutes(-29);
        var ended = await CreatePostAsync(hostId, EventType.Bar, slots: null, ct, justStarted, justStarted.AddMinutes(15));
        using var host = ClientFor(hostId);
        using var guest = ClientFor(NewUser("guest"));

        // Act
        using var own = await host.PostAsync($"/api/v1/attendance/{hosted.Value}/request", content: null, ct);
        using var past = await guest.PostAsync($"/api/v1/attendance/{ended.Value}/request", content: null, ct);
        using var missing = await guest.PostAsync("/api/v1/attendance/999999/request", content: null, ct);

        // Assert
        using (new AssertionScope())
        {
            own.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            (await ErrorCodeAsync(own, ct)).Should().Be("attendance.participation.own_post");
            past.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            (await ErrorCodeAsync(past, ct)).Should().Be("attendance.participation.post_ended");
            missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await ErrorCodeAsync(missing, ct)).Should().Be("attendance.participation.post_not_found");
        }
    }

    [Fact]
    public async Task GivenAHousePartyWithOneSlot_WhenTheHostAcceptsTwoGuests_ThenTheSecondStaysPendingAndNewRequestsAreRefused()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var post = await CreatePostAsync(hostId, EventType.HouseParty, slots: 1, ct);
        var (first, second, late) = (NewUser("first"), NewUser("second"), NewUser("late"));
        await RequestAsync(post, first, ct);
        await RequestAsync(post, second, ct);
        using var host = ClientFor(hostId);
        using var lateGuest = ClientFor(late);

        // Act
        using var accepted = await host.PostAsync($"/api/v1/attendance/{post.Value}/accept/{first}", content: null, ct);
        using var full = await host.PostAsync($"/api/v1/attendance/{post.Value}/accept/{second}", content: null, ct);
        using var tooLate = await lateGuest.PostAsync($"/api/v1/attendance/{post.Value}/request", content: null, ct);

        // Assert
        using (new AssertionScope())
        {
            accepted.StatusCode.Should().Be(HttpStatusCode.OK);
            full.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            (await ErrorCodeAsync(full, ct)).Should().Be("attendance.participation.no_slots_left");
            (await StatusAsync(post, second, ct)).Should().Be(AttendanceStatus.Pending);
            tooLate.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            (await ErrorCodeAsync(tooLate, ct)).Should().Be("attendance.participation.no_slots_left");
        }
    }

    [Fact]
    public async Task GivenAnAcceptedGuest_WhenTheHostRemovesThemAndTheyAskAgainThenCancelTwice_ThenTheLegacyFlowHolds()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var guestId = NewUser("guest");
        var post = await CreatePostAsync(hostId, EventType.HouseParty, slots: null, ct);
        await RequestAsync(post, guestId, ct);
        using var host = ClientFor(hostId);
        using var guest = ClientFor(guestId);
        using var accepted = await host.PostAsync($"/api/v1/attendance/{post.Value}/accept/{guestId}", content: null, ct);

        // Act
        using var removed = await host.PostAsync($"/api/v1/attendance/{post.Value}/reject/{guestId}", content: null, ct);
        var afterRemoval = await StatusAsync(post, guestId, ct);
        using var again = await guest.PostAsync($"/api/v1/attendance/{post.Value}/request", content: null, ct);
        var afterRequest = await StatusAsync(post, guestId, ct);
        using var cancelled = await guest.PostAsync($"/api/v1/attendance/{post.Value}/cancel", content: null, ct);
        using var cancelledTwice = await guest.PostAsync($"/api/v1/attendance/{post.Value}/cancel", content: null, ct);

        // Assert
        using (new AssertionScope())
        {
            accepted.StatusCode.Should().Be(HttpStatusCode.OK);
            removed.StatusCode.Should().Be(HttpStatusCode.OK);
            afterRemoval.Should().Be(AttendanceStatus.Rejected);
            again.StatusCode.Should().Be(HttpStatusCode.OK);
            afterRequest.Should().Be(AttendanceStatus.Pending);
            cancelled.StatusCode.Should().Be(HttpStatusCode.OK);
            cancelledTwice.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await StatusAsync(post, guestId, ct)).Should().Be(AttendanceStatus.Cancelled);
        }
    }

    [Fact]
    public async Task GivenTwoDecisionsOnOneParticipation_WhenTheSecondIsSavedAfterTheFirst_ThenItIsAConflict()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var guestId = NewUser("guest");
        var post = await CreatePostAsync(NewUser("host"), EventType.HouseParty, slots: null, ct);
        await RequestAsync(post, guestId, ct);
        await using var first = factory.Services.CreateAsyncScope();
        await using var second = factory.Services.CreateAsyncScope();
        var terms = await TermsAsync(post, ct);
        var byFirst = await first.ServiceProvider.GetRequiredService<IAttendanceRepository>().GetAsync(post, UserId.From(guestId), ct);
        var bySecond = await second.ServiceProvider.GetRequiredService<IAttendanceRepository>().GetAsync(post, UserId.From(guestId), ct);
        byFirst!.Accept(terms, acceptedAttendees: 0, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        await first.ServiceProvider.GetRequiredService<IAttendanceUnitOfWork>().SaveChangesAsync(ct);
        bySecond!.Reject(DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();

        // Act
        var act = () => second.ServiceProvider.GetRequiredService<IAttendanceUnitOfWork>().SaveChangesAsync(ct);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyConflictException>();
        (await StatusAsync(post, guestId, ct)).Should().Be(AttendanceStatus.Accepted);
    }

    private static string NewUser(string role) => $"attendance-{role}-{Guid.NewGuid():N}";

    [Fact]
    public async Task GivenAcceptedAndPendingGuests_WhenTheAttendeesAreListed_ThenAcceptedArePublicAndPendingOnlyForTheHost()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var post = await CreatePostAsync(hostId, EventType.HouseParty, slots: null, ct);
        var (first, second, waiting) = (NewUser("first"), NewUser("second"), NewUser("waiting"));
        await RequestAsync(post, first, ct);
        await RequestAsync(post, second, ct);
        await RequestAsync(post, waiting, ct);
        using var host = ClientFor(hostId);
        using var stranger = ClientFor(NewUser("stranger"));
        (await host.PostAsync($"/api/v1/attendance/{post.Value}/accept/{first}", content: null, ct)).EnsureSuccessStatusCode();
        (await host.PostAsync($"/api/v1/attendance/{post.Value}/accept/{second}", content: null, ct)).EnsureSuccessStatusCode();

        // Act
        using var firstPage = JsonDocument.Parse(await stranger.GetStringAsync($"/api/v1/posts/{post.Value}/attendees?limit=1", ct));
        var cursor = firstPage.RootElement.GetProperty("nextCursor").GetString();
        using var secondPage = JsonDocument.Parse(await stranger.GetStringAsync($"/api/v1/posts/{post.Value}/attendees?limit=1&cursor={cursor}", ct));
        using var pendingForStranger = await stranger.GetAsync($"/api/v1/posts/{post.Value}/attendees?status=pending", ct);
        using var pending = JsonDocument.Parse(await host.GetStringAsync($"/api/v1/posts/{post.Value}/attendees?status=pending", ct));
        using var unknown = await host.GetAsync($"/api/v1/posts/{post.Value}/attendees?status=rejected", ct);

        // Assert
        var attendee = firstPage.RootElement.GetProperty("items")[0];
        using (new AssertionScope())
        {
            attendee.GetProperty("userId").GetString().Should().Be(first);
            attendee.GetProperty("status").GetString().Should().Be("accepted");
            attendee.GetProperty("requestedAt").GetDateTimeOffset().Offset.Should().Be(TimeSpan.Zero);
            attendee.GetProperty("respondedAt").ValueKind.Should().Be(JsonValueKind.String);
            UserIds(secondPage).Should().Equal(second);
            secondPage.RootElement.GetProperty("nextCursor").ValueKind.Should().Be(JsonValueKind.Null);
            pendingForStranger.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await ErrorCodeAsync(pendingForStranger, ct)).Should().Be("attendance.participation.not_post_host");
            UserIds(pending).Should().Equal(waiting);
            unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task GivenAGuestWithRunningDisabledAndPendingAttendances_WhenTheyListThem_ThenOnlyTheirAcceptedOnesAreSplitByStatus()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var hostId = NewUser("host");
        var guestId = NewUser("guest");
        var running = await CreatePostAsync(hostId, EventType.Bar, slots: null, ct);
        var disabled = await CreatePostAsync(hostId, EventType.Club, slots: null, ct, new DateTimeOffset(2030, 2, 1, 18, 0, 0, TimeSpan.Zero));
        var awaiting = await CreatePostAsync(hostId, EventType.HouseParty, slots: null, ct);
        var othersOnly = await CreatePostAsync(hostId, EventType.Bar, slots: null, ct);
        await RequestAsync(running, guestId, ct);
        await RequestAsync(disabled, guestId, ct);
        await RequestAsync(awaiting, guestId, ct);
        await RequestAsync(othersOnly, NewUser("other"), ct);
        using var admin = factory.CreateClient();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(NewUser("admin"), [HopautRoles.Admin]));
        using var disabling = await admin.PatchAsJsonAsync($"/api/v1/posts/{disabled.Value}/status", new { status = "disabled" }, ct);
        using var guest = ClientFor(guestId);

        // Act
        using var active = JsonDocument.Parse(await guest.GetStringAsync("/api/v1/users/me/attendances", ct));
        using var inactive = JsonDocument.Parse(await guest.GetStringAsync("/api/v1/users/me/attendances?status=inactive", ct));

        // Assert
        using (new AssertionScope())
        {
            disabling.StatusCode.Should().Be(HttpStatusCode.NoContent);
            PostIds(active).Should().Equal(running.Value);
            PostIds(inactive).Should().Equal(disabled.Value);
        }
    }

    private static List<string?> UserIds(JsonDocument page) =>
        page.RootElement.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("userId").GetString()).ToList();

    private static List<int> PostIds(JsonDocument page) =>
        page.RootElement.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();

    private HttpClient ClientFor(string userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(userId));
        return client;
    }

    private async Task<PostId> CreatePostAsync(
        string hostId, EventType type, int? slots, CancellationToken ct, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var eventTime = start ?? new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero);
        var created = await TestCaller.SendAsync(factory.Services, caller: null, new CreatePostCommand(
            eventTime,
            end ?? eventTime.AddHours(5),
            UserId.From(hostId),
            new CreatePostLocationInput("Venue", "Berlin", null, "Main St 1", "DE", 13.405, 52.52),
            new CreatePostEventInput(type, null, null, "Attendance", "attendance endpoint tests", null, slots, null)), ct);
        return created.Value;
    }

    private async Task RequestAsync(PostId post, string guestId, CancellationToken ct)
    {
        using var guest = ClientFor(guestId);
        using var response = await guest.PostAsync($"/api/v1/attendance/{post.Value}/request", content: null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<AttendanceTerms> TermsAsync(PostId post, CancellationToken ct)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var terms = await scope.ServiceProvider.GetRequiredService<IPostAttendanceTermsQueries>().GetAsync(post, ct);
        return new AttendanceTerms(
            terms!.Host.PostId, terms.Host.HostId, terms.IsListed, terms.EndsAt, terms.HostApprovesAttendees, Slots: null);
    }

    private async Task<AttendanceStatus> StatusAsync(PostId post, string userId, CancellationToken ct)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AttendanceModuleDbContext>().Participations
            .Where(p => p.PostId == post && p.UserId == UserId.From(userId))
            .Select(p => p.Status)
            .SingleAsync(ct);
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return problem.RootElement.TryGetProperty("errorCode", out var code) ? code.GetString() : null;
    }
}
