using Hopaut.Modules.Posts.Domain.Events;
using Hopaut.SharedKernel;
using NetTopologySuite.Geometries;

namespace Hopaut.Modules.Posts.Domain;

public sealed class Post : AggregateRoot<PostId>
{
    public DateTimeOffset PostTime { get; private set; }
    public DateTimeOffset EventTime { get; private set; }
    public DateTimeOffset? EndTime { get; private set; }
    public int ActiveFlag { get; private set; }
    public UserId UserId { get; private set; }

    public EventLocation Location { get; private set; } = default!;
    public Event Event { get; private set; } = default!;
    public List<Picture> Pictures { get; private set; } = [];
    public List<PostTag> Tags { get; private set; } = [];
    public RepeatableProperty? Repeatable { get; private set; }

    private Post() { } // EF Core

    public static Post Create(
        UserId userId,
        DateTimeOffset eventTime,
        DateTimeOffset? endTime,
        EventLocation location,
        Event @event,
        List<Picture>? pictures,
        DateTimeOffset now)
    {
        var post = new Post
        {
            PostTime = now,
            EventTime = eventTime,
            EndTime = endTime,
            ActiveFlag = 1,
            UserId = userId,
            Location = location,
            Event = @event,
            Pictures = pictures ?? []
        };

        post.Raise(new PostCreatedDomainEvent(now, post.Id, userId));

        return post;
    }
}
