using Hopaut.SharedKernel;
using NetTopologySuite.Geometries;
using Coordinates = Hopaut.SharedKernel.Coordinates;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>Where a post's event takes place: a WGS-84 point (SRID 4326, PostGIS geography) and optional address parts.</summary>
public sealed class EventLocation
{
    public const int Wgs84 = 4326;

    private EventLocation() { } // EF Core

    public int Id { get; private set; }

    public PostId PostId { get; private set; }

    public string? EntityName { get; private set; }

    public string? City { get; private set; }

    public string? Region { get; private set; }

    public string? Address { get; private set; }

    public string? Country { get; private set; }

    public Point Location { get; private set; }

    internal static EventLocation Create(PostId postId, Coordinates coordinates, PostPlace place) => new()
    {
        PostId = postId,
        EntityName = place.EntityName,
        City = place.City,
        Region = place.Region,
        Address = place.Address,
        Country = place.Country,
        Location = new Point(coordinates.Longitude, coordinates.Latitude) { SRID = Wgs84 },
    };
}
