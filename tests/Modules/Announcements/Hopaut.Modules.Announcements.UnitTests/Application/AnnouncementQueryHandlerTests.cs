using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Announcements.Application.Queries;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementById;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;
using Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementThreads;
using Hopaut.Modules.Announcements.Application.Queries.GetPostAnnouncements;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using NSubstitute;
using static Hopaut.Modules.Announcements.UnitTests.AnnouncementTestData;

namespace Hopaut.Modules.Announcements.UnitTests.Application;

public sealed class AnnouncementQueryHandlerTests
{
    private readonly IAnnouncementReadModel _readModel = Substitute.For<IAnnouncementReadModel>();
    private readonly IPostHostQueries _posts = Substitute.For<IPostHostQueries>();
    private readonly IAttendanceQueries _attendance = Substitute.For<IAttendanceQueries>();
    private readonly IResourceAuthorizer _authorizer = Substitute.For<IResourceAuthorizer>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    public AnnouncementQueryHandlerTests() => _currentUser.Id.Returns(Guest);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetById_ForAMissingAnnouncement_IsNotFoundAndLooksNothingUp()
    {
        // Act
        var result = await GetByIdHandler().Handle(new GetAnnouncementByIdQuery(Id), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(AnnouncementErrors.NotFound(Id));
            await _posts.DidNotReceive().GetAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetById_WhenThePostIsGone_IsPostNotFound()
    {
        // Arrange
        _readModel.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(Dto(Id.Value, Now));

        // Act
        var result = await GetByIdHandler().Handle(new GetAnnouncementByIdQuery(Id), Ct);

        // Assert
        result.Error.Should().Be(AnnouncementErrors.PostNotFound(Post));
    }

    [Fact]
    public async Task GetById_ForTheHostOrAnAdministrator_ReturnsItWithoutLookingUpTheAttendance()
    {
        // Arrange
        var announcement = Dto(Id.Value, Now);
        _readModel.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(announcement);
        PostHosted(callerHostsOrAdministers: true);

        // Act
        var result = await GetByIdHandler().Handle(new GetAnnouncementByIdQuery(Id), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Should().Be(announcement);
            await _attendance.DidNotReceive().IsAcceptedAsync(Arg.Any<PostId>(), Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetById_ForAnAcceptedAttendee_ReturnsIt()
    {
        // Arrange
        var announcement = Dto(Id.Value, Now);
        _readModel.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(announcement);
        PostHosted(callerHostsOrAdministers: false, callerAttends: true);

        // Act
        var result = await GetByIdHandler().Handle(new GetAnnouncementByIdQuery(Id), Ct);

        // Assert
        result.Value.Should().Be(announcement);
    }

    [Fact]
    public async Task GetById_ForSomeoneOutsideThePostsAudience_IsForbidden()
    {
        // Arrange
        _readModel.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(Dto(Id.Value, Now));
        PostHosted(callerHostsOrAdministers: false, callerAttends: false);

        // Act
        var result = await GetByIdHandler().Handle(new GetAnnouncementByIdQuery(Id), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(AnnouncementErrors.NotPostAudience(Post));
            result.Error.Type.Should().Be(ErrorType.Forbidden);
        }
    }

    [Fact]
    public async Task GetByPost_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await GetByPostHandler().Handle(new GetAnnouncementsByPostQuery(Post), Ct);

        // Assert
        result.Error.Should().Be(AnnouncementErrors.PostNotFound(Post));
    }

    [Fact]
    public async Task GetByPost_ForSomeoneOutsideThePostsAudience_IsForbiddenAndReadsNothing()
    {
        // Arrange
        PostHosted(callerHostsOrAdministers: false, callerAttends: false);

        // Act
        var result = await GetByPostHandler().Handle(new GetAnnouncementsByPostQuery(Post), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(AnnouncementErrors.NotPostAudience(Post));
            await _readModel.DidNotReceive().GetByPostAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetByPost_ForAnAcceptedAttendee_ReturnsTheAnnouncements()
    {
        // Arrange
        PostHosted(callerHostsOrAdministers: false, callerAttends: true);
        IReadOnlyList<AnnouncementDto> announcements = [Dto(2, Now.AddMinutes(1)), Dto(1, Now)];
        _readModel.GetByPostAsync(Post, Arg.Any<CancellationToken>()).Returns(announcements);

        // Act
        var result = await GetByPostHandler().Handle(new GetAnnouncementsByPostQuery(Post), Ct);

        // Assert
        result.Value.Should().Equal(announcements);
    }

    [Fact]
    public async Task GetPostAnnouncements_WithAnInvalidCursorAndLimit_ReportsBothAndLooksNothingUp()
    {
        // Act
        var result = await GetPageHandler().Handle(new GetPostAnnouncementsQuery(Post, "%%", 0), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("cursor", "limit");
            await _posts.DidNotReceive().GetAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetPostAnnouncements_ForSomeoneOutsideThePostsAudience_IsForbidden()
    {
        // Arrange
        PostHosted(callerHostsOrAdministers: false, callerAttends: false);

        // Act
        var result = await GetPageHandler().Handle(new GetPostAnnouncementsQuery(Post, null, null), Ct);

        // Assert
        result.Error.Should().Be(AnnouncementErrors.NotPostAudience(Post));
    }

    [Fact]
    public async Task GetPostAnnouncements_ReturnsOnePageAndTheCursorOfItsLastItem()
    {
        // Arrange
        PostHosted(callerHostsOrAdministers: true);
        _readModel.GetPageAsync(Post, null, 3, Arg.Any<CancellationToken>())
            .Returns([Dto(3, Now.AddMinutes(2)), Dto(2, Now.AddMinutes(1)), Dto(1, Now)]);

        // Act
        var result = await GetPageHandler().Handle(new GetPostAnnouncementsQuery(Post, null, 2), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.Id.Value).Should().Equal(3, 2);
            AnnouncementKeyset.Decode(result.Value.NextCursor).Value.Should().Be(new AnnouncementKeyset(Now.AddMinutes(1), AnnouncementId.From(2)));
        }
    }

    [Fact]
    public async Task GetPostAnnouncements_OnTheLastPage_HasNoCursorAndContinuesAfterTheGivenOne()
    {
        // Arrange
        PostHosted(callerHostsOrAdministers: false, callerAttends: true);
        var after = new AnnouncementKeyset(Now.AddMinutes(1), AnnouncementId.From(2));
        _readModel.GetPageAsync(Post, after, PageSize.Default.Value + 1, Arg.Any<CancellationToken>()).Returns([Dto(1, Now)]);

        // Act
        var result = await GetPageHandler().Handle(new GetPostAnnouncementsQuery(Post, after.Encode().Value, null), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.Id.Value).Should().Equal(1);
            result.Value.NextCursor.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetThreads_WithInvalidInputs_ReportsEachFieldAndReadsNothing()
    {
        // Act
        var result = await GetThreadsHandler().Handle(new GetAnnouncementThreadsQuery("sent", "%%", 101), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("box", "cursor", "limit");
            await _attendance.DidNotReceive().GetAcceptedPostIdsAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetThreads_InTheInbox_ListsThePostsTheCallerAttends()
    {
        // Arrange
        IReadOnlyList<PostId> attended = [Post];
        _attendance.GetAcceptedPostIdsAsync(Guest, Arg.Any<CancellationToken>()).Returns(attended);
        var thread = new AnnouncementThread(Post, Dto(1, Now));
        _readModel.GetThreadsAsync(attended, null, PageSize.Default.Value + 1, Arg.Any<CancellationToken>()).Returns([thread]);

        // Act
        var result = await GetThreadsHandler().Handle(new GetAnnouncementThreadsQuery(null, null, null), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Should().Equal(thread);
            result.Value.NextCursor.Should().BeNull();
            await _posts.DidNotReceive().GetHostedPostIdsAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task GetThreads_InTheOutbox_ListsThePostsTheCallerHostsOnePageAtATime()
    {
        // Arrange
        IReadOnlyList<PostId> hosted = [Post, PostId.From(8)];
        _posts.GetHostedPostIdsAsync(Guest, Arg.Any<CancellationToken>()).Returns(hosted);
        var after = new AnnouncementThreadKeyset(Now.AddHours(1), PostId.From(9));
        _readModel.GetThreadsAsync(hosted, after, 2, Arg.Any<CancellationToken>())
            .Returns([new AnnouncementThread(PostId.From(8), Dto(2, Now.AddMinutes(5))), new AnnouncementThread(Post, Dto(1, Now))]);

        // Act
        var result = await GetThreadsHandler().Handle(new GetAnnouncementThreadsQuery("Outbox", after.Encode().Value, 1), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Items.Select(item => item.PostId.Value).Should().Equal(8);
            AnnouncementThreadKeyset.Decode(result.Value.NextCursor).Value.Should().Be(new AnnouncementThreadKeyset(Now.AddMinutes(5), PostId.From(8)));
            await _attendance.DidNotReceive().GetAcceptedPostIdsAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public void AnnouncementKeyset_EncodedAndDecoded_RoundTrips()
    {
        // Arrange
        var keyset = new AnnouncementKeyset(Now, AnnouncementId.From(12));

        // Act
        var decoded = AnnouncementKeyset.Decode(keyset.Encode().Value);

        // Assert
        decoded.Value.Should().Be(keyset);
    }

    [Fact]
    public void AnnouncementThreadKeyset_EncodedAndDecoded_RoundTrips()
    {
        // Arrange
        var keyset = new AnnouncementThreadKeyset(Now, PostId.From(12));

        // Act
        var decoded = AnnouncementThreadKeyset.Decode(keyset.Encode().Value);

        // Assert
        decoded.Value.Should().Be(keyset);
    }

    [Theory]
    [InlineData("not base64!")]
    [InlineData("MTIzNA")] // "1234": no separator
    [InlineData("MTIzNDow")] // "1234:0": no announcement or post has ID 0
    [InlineData("eDox")] // "x:1": no tick count
    [InlineData("OTAwMDAwMDAwMDAwMDAwMDAwMDox")] // "9000000000000000000:1": beyond the last representable time
    [InlineData("OTk5OTk5OTk5OTk5OTk5OTk5OTk6MQ")] // "99999999999999999999:1": not a 64-bit tick count
    public void Keysets_DecodingAnUnreadableCursor_AreInvalidCursors(string value)
    {
        // Act
        var announcement = AnnouncementKeyset.Decode(value);
        var thread = AnnouncementThreadKeyset.Decode(value);

        // Assert
        using (new AssertionScope())
        {
            announcement.Error.Should().Be(PagingErrors.InvalidCursor);
            thread.Error.Should().Be(PagingErrors.InvalidCursor);
        }
    }

    [Theory]
    [InlineData(null, AnnouncementBox.Inbox)]
    [InlineData("inbox", AnnouncementBox.Inbox)]
    [InlineData("OUTBOX", AnnouncementBox.Outbox)]
    public void ParseBox_ReadsTheWireValueCaseInsensitively(string? box, AnnouncementBox expected) =>
        AnnouncementThreads.ParseBox(box).Value.Should().Be(expected);

    [Fact]
    public void ParseBox_OfAnotherValue_IsUnknown() =>
        AnnouncementThreads.ParseBox("sent").Error.Should().Be(AnnouncementThreads.UnknownBox);

    private GetAnnouncementByIdQueryHandler GetByIdHandler() => new(_readModel, _posts, _attendance, _authorizer, _currentUser);

    private GetAnnouncementsByPostQueryHandler GetByPostHandler() => new(_readModel, _posts, _attendance, _authorizer, _currentUser);

    private GetPostAnnouncementsQueryHandler GetPageHandler() => new(_readModel, _posts, _attendance, _authorizer, _currentUser);

    private GetAnnouncementThreadsQueryHandler GetThreadsHandler() => new(_readModel, _attendance, _posts, _currentUser);

    private void PostHosted(bool callerHostsOrAdministers, bool callerAttends = false)
    {
        var host = new PostHost(Post, Host);
        _posts.GetAsync(Post, Arg.Any<CancellationToken>()).Returns(host);
        _authorizer.IsAuthorizedAsync(host, AnnouncementPolicies.MustOwnAnnouncement, Arg.Any<CancellationToken>()).Returns(callerHostsOrAdministers);
        _attendance.IsAcceptedAsync(Post, Guest, Arg.Any<CancellationToken>()).Returns(callerAttends);
    }

    private static AnnouncementDto Dto(int id, DateTimeOffset createdAt) =>
        new(AnnouncementId.From(id), Post, $"Announcement {id}", createdAt, EditedAt: null);
}
