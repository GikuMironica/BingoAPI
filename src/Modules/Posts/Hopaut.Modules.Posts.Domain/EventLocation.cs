using Hopaut.SharedKernel;
using NetTopologySuite.Geometries;

namespace Hopaut.Modules.Posts.Domain;

public sealed class EventLocation
{
    public int Id { get; set; }
    public string? EntityName { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public Point Location { get; set; } = default!;

    public PostId PostId { get; set; }
    public Post? Post { get; set; }
}
