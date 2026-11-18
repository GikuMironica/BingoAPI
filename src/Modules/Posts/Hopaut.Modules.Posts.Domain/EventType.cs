namespace Hopaut.Modules.Posts.Domain;

/// <summary>
/// Collapsed event type enum replacing the 9-class TPH hierarchy.
/// </summary>
public enum EventType
{
    HouseParty = 1,
    Bar = 2,
    Club = 3,
    CarMeet = 4,
    BicycleMeet = 5,
    BikerMeet = 6,
    Sport = 7,
    StreetParty = 8,
    Other = 9
}

/// <summary>Rules that depend on the kind of event.</summary>
public static class EventTypeRules
{
    /// <summary>House parties are private, so their host approves each attendee; other events admit attendees on request (legacy API).</summary>
    public static bool HostApprovesAttendees(this EventType type) => type == EventType.HouseParty;
}
