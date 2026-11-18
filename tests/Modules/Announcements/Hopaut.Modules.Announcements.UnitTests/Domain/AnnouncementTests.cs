using AwesomeAssertions.Execution;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Announcements.Domain.Events;
using static Hopaut.Modules.Announcements.UnitTests.AnnouncementTestData;

namespace Hopaut.Modules.Announcements.UnitTests.Domain;

public sealed class AnnouncementTests
{
    [Fact]
    public void Create_OnAPost_AnnouncesTheMessageAndRaisesCreated()
    {
        // Act
        var announcement = Announcement.Create(Id, Post, Message("Doors open at eight"), Now);

        // Assert
        using (new AssertionScope())
        {
            announcement.Id.Should().Be(Id);
            announcement.PostId.Should().Be(Post);
            announcement.Message.Value.Should().Be("Doors open at eight");
            announcement.CreatedAt.Should().Be(Now);
            announcement.EditedAt.Should().BeNull();
            announcement.DomainEvents.Should().ContainSingle().Which.Should().BeEquivalentTo(
                new AnnouncementCreatedDomainEvent(Now, Id, Post), options => options.Excluding(e => e.EventId));
        }
    }

    [Fact]
    public void Create_WithoutAnId_IsAProgrammingError()
    {
        // Act
        var act = () => Announcement.Create(default, Post, Message("Doors open at eight"), Now);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithoutAMessage_IsAProgrammingError()
    {
        // Act
        var act = () => Announcement.Create(Id, Post, message: null!, Now);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Edit_WithANewMessage_ReplacesItRecordsTheTimeAndRaisesEdited()
    {
        // Arrange
        var announcement = Existing();
        var later = Now.AddHours(1);

        // Act
        announcement.Edit(Message("Doors open at nine"), later);

        // Assert
        using (new AssertionScope())
        {
            announcement.Message.Value.Should().Be("Doors open at nine");
            announcement.CreatedAt.Should().Be(Now);
            announcement.EditedAt.Should().Be(later);
            announcement.DomainEvents.Should().ContainSingle().Which.Should().BeEquivalentTo(
                new AnnouncementEditedDomainEvent(later, Id, Post), options => options.Excluding(e => e.EventId));
        }
    }

    [Fact]
    public void Edit_WithTheCurrentMessage_ChangesNothing()
    {
        // Arrange
        var announcement = Existing("Doors open at eight");

        // Act
        announcement.Edit(Message("  Doors open at eight "), Now.AddHours(1));

        // Assert
        using (new AssertionScope())
        {
            announcement.EditedAt.Should().BeNull();
            announcement.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Edit_WithoutAMessage_IsAProgrammingError()
    {
        // Arrange
        var announcement = Existing();

        // Act
        var act = () => announcement.Edit(message: null!, Now);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Delete_RaisesDeleted()
    {
        // Arrange
        var announcement = Existing();
        var later = Now.AddDays(1);

        // Act
        announcement.Delete(later);

        // Assert
        announcement.DomainEvents.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new AnnouncementDeletedDomainEvent(later, Id, Post), options => options.Excluding(e => e.EventId));
    }
}
