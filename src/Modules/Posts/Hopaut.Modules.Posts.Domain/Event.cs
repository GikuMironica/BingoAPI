using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>
/// Collapsed Event entity — replaces the 9-subclass TPH hierarchy.
/// Type-specific data is stored as JSONB via <see cref="TypeSpecificData"/>.
/// </summary>
public sealed class Event
{
    public int Id { get; set; }
    public EventType EventType { get; set; }
    public double? EntrancePrice { get; set; }
    public int? Currency { get; set; }
    public string? Title { get; set; }
    public string Description { get; set; } = default!;
    public string? Requirements { get; set; }
    public int? Slots { get; set; }

    /// <summary>
    /// JSONB column for type-specific properties (e.g., "dressCode", "musicGenre").
    /// </summary>
    public string? TypeSpecificData { get; set; }

    public PostId PostId { get; set; }
    public Post? Post { get; set; }
}
