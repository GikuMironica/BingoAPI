namespace Hopaut.Modules.Ratings.Domain;

public sealed class Rating
{
    public int Id { get; set; }
    public int Rate { get; set; }
    public string UserId { get; set; } = default!;
    public string RaterId { get; set; } = default!;
    public int PostId { get; set; }
    public string? Feedback { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
