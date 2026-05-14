namespace Hopaut.Modules.Announcements.Domain;

public sealed class Announcement
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Message { get; set; } = default!;
    public long Timestamp { get; set; }
}
