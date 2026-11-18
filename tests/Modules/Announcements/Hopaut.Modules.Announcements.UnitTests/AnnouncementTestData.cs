using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.UnitTests;

/// <summary>Stable test data: one post hosted by <see cref="Host"/> and attended by <see cref="Guest"/>.</summary>
internal static class AnnouncementTestData
{
    public static readonly DateTimeOffset Now = new(2030, 1, 9, 12, 0, 0, TimeSpan.Zero);
    public static readonly PostId Post = PostId.From(7);
    public static readonly UserId Host = UserId.From("host-1");
    public static readonly UserId Guest = UserId.From("guest-1");
    public static readonly AnnouncementId Id = AnnouncementId.From(41);

    public static AnnouncementMessage Message(string text) => AnnouncementMessage.Create(text).Value;

    /// <summary>An announcement of <see cref="Post"/> without pending domain events.</summary>
    public static Announcement Existing(string text = "Doors open at eight")
    {
        var announcement = Announcement.Create(Id, Post, Message(text), Now);
        announcement.ClearDomainEvents();
        return announcement;
    }
}
