using Hopaut.Modules.Posts.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>
/// A hosted event post. It owns its event details, location, pictures, tags and repeat settings; they change only through
/// its verbs. Schedule rules (legacy API): an end is required, the event lasts at least 15 minutes and less than 12 hours,
/// and it starts at most 30 minutes in the past.
/// </summary>
public sealed class Post : AggregateRoot<PostId>
{
    public static readonly TimeSpan MinimumDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan MaximumDuration = TimeSpan.FromHours(12);
    public static readonly TimeSpan StartTolerance = TimeSpan.FromMinutes(30);

    private readonly List<Picture> _pictures = [];
    private readonly List<PostTag> _tags = [];

    private Post() { } // EF Core

    public DateTimeOffset PostTime { get; private set; }

    public TimeRange Schedule { get; private set; }

    public PostStatus Status { get; private set; }

    public UserId UserId { get; private set; }

    public EventLocation Location { get; private set; }

    public Event Event { get; private set; }

    public IReadOnlyList<Picture> Pictures => _pictures;

    public IReadOnlyList<PostTag> Tags => _tags;

    public RepeatableProperty? Repeatable { get; private set; }

    /// <summary>Creates a post with an ID taken from the posts sequence, so <see cref="PostCreatedDomainEvent"/> carries it.</summary>
    public static Result<Post> Create(
        PostId id,
        UserId host,
        TimeRange schedule,
        Coordinates coordinates,
        PostPlace place,
        EventDetails details,
        IEnumerable<string>? pictureUrls,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(coordinates);
        ArgumentNullException.ThrowIfNull(place);
        ArgumentNullException.ThrowIfNull(details);

        if (CheckSchedule(schedule, now) is { } scheduleError)
            return scheduleError;

        if (details.Requirements?.Length > PostErrors.MaxRequirementsLength)
            return PostErrors.RequirementsTooLong;

        var post = new Post
        {
            Id = id,
            PostTime = now,
            Schedule = schedule,
            Status = PostStatus.Active,
            UserId = host,
            Location = EventLocation.Create(id, coordinates, place),
            Event = Event.Create(id, details),
        };

        foreach (var url in pictureUrls ?? [])
        {
            post._pictures.Add(Picture.Create(id, url));
        }

        post.Raise(new PostCreatedDomainEvent(now, id, host));
        return post;
    }

    public Result Reschedule(TimeRange schedule, DateTimeOffset now)
    {
        if (CheckSchedule(schedule, now) is { } scheduleError)
            return scheduleError;

        Schedule = schedule;
        return Result.Success();
    }

    /// <summary>Takes the post off every list (the legacy admin "disable" action).</summary>
    public Result Disable()
    {
        if (Status == PostStatus.Disabled)
            return PostErrors.AlreadyDisabled(Id);

        Status = PostStatus.Disabled;
        return Result.Success();
    }

    /// <summary>Announces the deletion; the repository removes the post in the same unit of work.</summary>
    public void Delete(DateTimeOffset now) => Raise(new PostDeletedDomainEvent(now, Id));

    public Picture AttachPicture(string url)
    {
        var picture = Picture.Create(Id, url);
        _pictures.Add(picture);
        return picture;
    }

    public Result MarkPictureReady(PictureId pictureId, string url, DateTimeOffset now) =>
        ChangePicture(pictureId, picture => picture.MarkReady(url), now);

    public Result MarkPictureFailed(PictureId pictureId, DateTimeOffset now) =>
        ChangePicture(pictureId, picture => picture.MarkFailed(), now);

    private Result ChangePicture(PictureId pictureId, Func<Picture, bool> change, DateTimeOffset now)
    {
        var picture = _pictures.Find(p => p.Id == pictureId);
        if (picture is null)
            return PostErrors.PictureNotFound(pictureId);

        if (!change(picture))
            return PostErrors.PictureNotPending(pictureId);

        Raise(new PictureStateChangedDomainEvent(now, pictureId, Id, picture.State));
        return Result.Success();
    }

    private static Error? CheckSchedule(TimeRange schedule, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (schedule.Duration is not { } duration)
            return PostErrors.EndRequired;

        if (duration < MinimumDuration)
            return PostErrors.TooShort;

        if (duration >= MaximumDuration)
            return PostErrors.TooLong;

        return schedule.Start < now - StartTolerance ? PostErrors.StartsInThePast : null;
    }
}
