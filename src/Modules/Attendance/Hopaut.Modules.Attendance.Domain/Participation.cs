namespace Hopaut.Modules.Attendance.Domain;

public sealed class Participation
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; } = default!;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}
