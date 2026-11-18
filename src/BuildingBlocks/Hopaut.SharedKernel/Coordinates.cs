namespace Hopaut.SharedKernel;

/// <summary>A WGS-84 position: latitude within ±90°, longitude within ±180°, both finite.</summary>
public sealed record Coordinates
{
    public const double MaxLatitude = 90;
    public const double MaxLongitude = 180;

    private Coordinates(double latitude, double longitude) => (Latitude, Longitude) = (latitude, longitude);

    public double Latitude { get; }

    public double Longitude { get; }

    public static Result<Coordinates> Create(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || Math.Abs(latitude) > MaxLatitude)
            return CoordinatesErrors.LatitudeOutOfRange;

        if (!double.IsFinite(longitude) || Math.Abs(longitude) > MaxLongitude)
            return CoordinatesErrors.LongitudeOutOfRange;

        return new Coordinates(latitude, longitude);
    }
}

public static class CoordinatesErrors
{
    public static readonly Error LatitudeOutOfRange =
        Error.Validation("shared.coordinates.latitude_out_of_range", "Latitude must be a number between -90 and 90 degrees.");

    public static readonly Error LongitudeOutOfRange =
        Error.Validation("shared.coordinates.longitude_out_of_range", "Longitude must be a number between -180 and 180 degrees.");
}
