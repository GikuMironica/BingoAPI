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
    public static readonly TimeSpan RescheduleTolerance = TimeSpan.FromMinutes(5);

    /// <summary>A running event must still last this long after an edit (legacy: 1700 s, 30 minutes less 100 s of clock slack).</summary>
    public static readonly TimeSpan MinimumRemainingTime = TimeSpan.FromSeconds(1700);
    public const int MaxTags = 20;

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

    /// <summary>Optimistic-concurrency token (PostgreSQL <c>xmin</c>): the host and administrators may edit a post at the same time.</summary>
    public uint Version { get; private set; }

    /// <summary>
    /// Creates a post with an ID taken from the posts sequence, so <see cref="PostCreatedDomainEvent"/> carries it. Tags are
    /// referenced by ID (duplicates collapse, at most <see cref="MaxTags"/>).
    /// </summary>
    public static Result<Post> Create(
        PostId id,
        UserId host,
        TimeRange schedule,
        Coordinates coordinates,
        PostPlace place,
        EventDetails details,
        IEnumerable<string>? pictureUrls,
        IEnumerable<TagId>? tags,
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

        var tagIds = (tags ?? []).Distinct().ToList();
        if (tagIds.Count > MaxTags)
            return PostErrors.TooManyTags;

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

        post._tags.AddRange(tagIds.Select(tagId => PostTag.Create(id, tagId)));

        post.Raise(new PostCreatedDomainEvent(now, id, host));
        return post;
    }

    /// <summary>Replaces the post's tags: links the new ones and unlinks the rest (duplicates collapse, at most <see cref="MaxTags"/>).</summary>
    public Result Retag(IEnumerable<TagId> tags, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(tags);
        return Update(new PostRevision { Tags = [.. tags] }, acceptedGuests: 0, now);
    }

    public Result Reschedule(TimeRange schedule, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        return Update(new PostRevision { Schedule = schedule }, acceptedGuests: 0, now);
    }

    /// <summary>
    /// Applies the host's edit with the legacy update rules: before the event starts, its start may move to at most
    /// <see cref="RescheduleTolerance"/> in the past; once it has started, only the end may move, to at least
    /// <see cref="MinimumRemainingTime"/> from now; the duration rules of <see cref="Create"/> apply; the capacity never drops
    /// below the accepted attendees. Nothing changes unless every rule holds. When something changed it raises
    /// <see cref="PostUpdatedDomainEvent"/>, asking to notify attendees if the time, place, requirements or fee changed.
    /// </summary>
    /// <param name="acceptedGuests">The post's accepted attendees; only checked when the revision changes the capacity.</param>
    public Result Update(PostRevision revision, int acceptedGuests, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(revision);

        if (revision.Schedule is { } schedule && CheckReschedule(schedule, now) is { } scheduleError)
            return scheduleError;

        if (revision.Requirements?.Length > PostErrors.MaxRequirementsLength)
            return PostErrors.RequirementsTooLong;

        if (revision.Slots is { } slots && slots.Value < acceptedGuests)
            return PostErrors.SlotsBelowAttendees(acceptedGuests);

        if (revision.Tags?.Distinct().Count() > MaxTags)
            return PostErrors.TooManyTags;

        var rescheduled = revision.Schedule is { } newSchedule && newSchedule != Schedule;
        Schedule = revision.Schedule ?? Schedule;
        var moved = revision.Location is { } location && Location.Move(location);
        var (revised, notifyOfEvent) = Event.Revise(revision);
        var retagged = revision.Tags is { } tags && RelinkTags(tags);

        if (rescheduled || moved || revised || retagged)
            Raise(new PostUpdatedDomainEvent(now, Id, NotifyAttendees: rescheduled || moved || notifyOfEvent));

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

    private static Error? CheckSchedule(TimeRange schedule, DateTimeOffset now) =>
        CheckDuration(schedule) ?? (schedule.Start < now - StartTolerance ? PostErrors.StartsInThePast : null);

    private Error? CheckReschedule(TimeRange schedule, DateTimeOffset now)
    {
        if (CheckDuration(schedule) is { } durationError)
            return durationError;

        if (Schedule.Start > now)
            return schedule.Start < now - RescheduleTolerance ? PostErrors.MovedToThePast : null;

        if (schedule.Start != Schedule.Start)
            return PostErrors.AlreadyStarted;

        return schedule.End < now + MinimumRemainingTime ? PostErrors.EndsTooSoon : null;
    }

    private static Error? CheckDuration(TimeRange schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (schedule.Duration is not { } duration)
            return PostErrors.EndRequired;

        if (duration < MinimumDuration)
            return PostErrors.TooShort;

        return duration >= MaximumDuration ? PostErrors.TooLong : null;
    }

    private bool RelinkTags(IEnumerable<TagId> tags)
    {
        var tagIds = tags.Distinct().ToList();
        var unlinked = _tags.RemoveAll(link => !tagIds.Contains(link.TagId));
        var linked = tagIds
            .Where(tagId => _tags.TrueForAll(link => link.TagId != tagId))
            .Select(tagId => PostTag.Create(Id, tagId))
            .ToList();
        _tags.AddRange(linked);
        return unlinked > 0 || linked.Count > 0;
    }
}
