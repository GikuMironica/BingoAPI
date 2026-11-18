using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;
using Hopaut.Modules.Announcements.Application.Commands.UpdateAnnouncement;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Announcements.Domain.Events;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using static Hopaut.Modules.Announcements.UnitTests.AnnouncementTestData;

namespace Hopaut.Modules.Announcements.UnitTests.Application;

public sealed class AnnouncementCommandHandlerTests
{
    private readonly IAnnouncementRepository _announcements = Substitute.For<IAnnouncementRepository>();
    private readonly IPostHostQueries _posts = Substitute.For<IPostHostQueries>();
    private readonly IResourceAuthorizer _authorizer = Substitute.For<IResourceAuthorizer>();
    private readonly FakeTimeProvider _time = new(Now);

    public AnnouncementCommandHandlerTests() => _announcements.NextIdAsync(Arg.Any<CancellationToken>()).Returns(Id);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Create_WithABlankMessage_ReportsTheMessageAndLooksNothingUp()
    {
        // Act
        var result = await CreateHandler().Handle(new CreateAnnouncementCommand(Post, "   "), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("message");
            await _posts.DidNotReceive().GetAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Create_ForAMissingPost_IsNotFound()
    {
        // Act
        var result = await CreateHandler().Handle(new CreateAnnouncementCommand(Post, "Doors open at eight"), Ct);

        // Assert
        result.Error.Should().Be(AnnouncementErrors.PostNotFound(Post));
    }

    [Fact]
    public async Task Create_BySomeoneOtherThanTheHostOrAnAdministrator_IsForbiddenAndAddsNothing()
    {
        // Arrange
        PostHosted(callerMayAnnounce: false);

        // Act
        var result = await CreateHandler().Handle(new CreateAnnouncementCommand(Post, "Doors open at eight"), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(AnnouncementErrors.NotPostHost(Post));
            _announcements.DidNotReceive().Add(Arg.Any<Announcement>());
        }
    }

    [Fact]
    public async Task Create_ByTheHostOrAnAdministrator_AddsTheTrimmedMessageUnderTheNextId()
    {
        // Arrange
        PostHosted(callerMayAnnounce: true);

        // Act
        var result = await CreateHandler().Handle(new CreateAnnouncementCommand(Post, "  Doors open at eight "), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Should().Be(Id);
            _announcements.Received(1).Add(Arg.Is<Announcement>(a =>
                a.Id == Id && a.PostId == Post && a.Message.Value == "Doors open at eight" && a.CreatedAt == Now));
        }
    }

    [Fact]
    public async Task Update_WithATooLongMessage_ReportsTheMessageAndLoadsNothing()
    {
        // Act
        var result = await UpdateHandler().Handle(new UpdateAnnouncementCommand(Id, new string('x', AnnouncementMessage.MaxLength + 1)), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().BeEquivalentTo("message");
            await _announcements.DidNotReceive().GetByIdAsync(Arg.Any<AnnouncementId>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Update_AMissingAnnouncement_IsNotFound()
    {
        // Act
        var result = await UpdateHandler().Handle(new UpdateAnnouncementCommand(Id, "Doors open at nine"), Ct);

        // Assert
        result.Error.Should().Be(AnnouncementErrors.NotFound(Id));
    }

    [Fact]
    public async Task Update_OfAnAnnouncementWhosePostIsGone_IsPostNotFound()
    {
        // Arrange
        _announcements.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(Existing());

        // Act
        var result = await UpdateHandler().Handle(new UpdateAnnouncementCommand(Id, "Doors open at nine"), Ct);

        // Assert
        result.Error.Should().Be(AnnouncementErrors.PostNotFound(Post));
    }

    [Fact]
    public async Task Update_BySomeoneOtherThanTheHostOrAnAdministrator_IsForbiddenAndKeepsTheMessage()
    {
        // Arrange
        var announcement = Stored(callerMayAnnounce: false);

        // Act
        var result = await UpdateHandler().Handle(new UpdateAnnouncementCommand(Id, "Doors open at nine"), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(AnnouncementErrors.NotPostHost(Post));
            announcement.Message.Value.Should().Be("Doors open at eight");
            announcement.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Update_ByTheHostOrAnAdministrator_EditsTheMessage()
    {
        // Arrange
        var announcement = Stored(callerMayAnnounce: true);

        // Act
        var result = await UpdateHandler().Handle(new UpdateAnnouncementCommand(Id, " Doors open at nine "), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            announcement.Message.Value.Should().Be("Doors open at nine");
            announcement.EditedAt.Should().Be(Now);
            announcement.DomainEvents.OfType<AnnouncementEditedDomainEvent>().Should().ContainSingle();
        }
    }

    [Fact]
    public async Task Delete_AMissingAnnouncement_IsNotFound()
    {
        // Act
        var result = await DeleteHandler().Handle(new DeleteAnnouncementCommand(Id), Ct);

        // Assert
        result.Error.Should().Be(AnnouncementErrors.NotFound(Id));
    }

    [Fact]
    public async Task Delete_BySomeoneOtherThanTheHostOrAnAdministrator_IsForbiddenAndKeepsIt()
    {
        // Arrange
        var announcement = Stored(callerMayAnnounce: false);

        // Act
        var result = await DeleteHandler().Handle(new DeleteAnnouncementCommand(Id), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Should().Be(AnnouncementErrors.NotPostHost(Post));
            _announcements.DidNotReceive().Remove(Arg.Any<Announcement>());
            announcement.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Delete_ByTheHostOrAnAdministrator_RemovesItAndRaisesDeleted()
    {
        // Arrange
        var announcement = Stored(callerMayAnnounce: true);

        // Act
        var result = await DeleteHandler().Handle(new DeleteAnnouncementCommand(Id), Ct);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            _announcements.Received(1).Remove(announcement);
            announcement.DomainEvents.OfType<AnnouncementDeletedDomainEvent>().Should().ContainSingle();
        }
    }

    private CreateAnnouncementCommandHandler CreateHandler() => new(_announcements, _posts, _authorizer, _time);

    private UpdateAnnouncementCommandHandler UpdateHandler() => new(_announcements, _posts, _authorizer, _time);

    private DeleteAnnouncementCommandHandler DeleteHandler() => new(_announcements, _posts, _authorizer, _time);

    private void PostHosted(bool callerMayAnnounce)
    {
        var host = new PostHost(Post, Host);
        _posts.GetAsync(Post, Arg.Any<CancellationToken>()).Returns(host);
        _authorizer.IsAuthorizedAsync(host, AnnouncementPolicies.MustOwnAnnouncement, Arg.Any<CancellationToken>()).Returns(callerMayAnnounce);
    }

    private Announcement Stored(bool callerMayAnnounce)
    {
        PostHosted(callerMayAnnounce);
        var announcement = Existing("Doors open at eight");
        _announcements.GetByIdAsync(Id, Arg.Any<CancellationToken>()).Returns(announcement);
        return announcement;
    }
}
