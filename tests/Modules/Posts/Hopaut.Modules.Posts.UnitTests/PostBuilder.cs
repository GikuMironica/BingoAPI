using System.Reflection;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.UnitTests;

/// <summary>Builds valid posts for tests; every input can be overridden.</summary>
internal sealed class PostBuilder
{
    public static readonly DateTimeOffset Now = new(2024, 1, 9, 12, 0, 0, TimeSpan.Zero);

    private PostId _id = PostId.From(7);
    private UserId _host = UserId.From("host-1");
    private DateTimeOffset _start = Now.AddDays(1);
    private DateTimeOffset? _end = Now.AddDays(1).AddHours(4);
    private string? _requirements;
    private IEnumerable<string>? _pictures;
    private IEnumerable<TagId>? _tags;
    private Money? _entranceFee;

    public PostBuilder WithId(int id) { _id = PostId.From(id); return this; }

    public PostBuilder HostedBy(string host) { _host = UserId.From(host); return this; }

    public PostBuilder From(DateTimeOffset start, DateTimeOffset? end) { (_start, _end) = (start, end); return this; }

    public PostBuilder WithRequirements(string requirements) { _requirements = requirements; return this; }

    public PostBuilder WithPictures(params string[] urls) { _pictures = urls; return this; }

    public PostBuilder WithTags(params int[] tagIds) { _tags = tagIds.Select(TagId.From).ToList(); return this; }

    public PostBuilder Charging(Money fee) { _entranceFee = fee; return this; }

    public Result<Post> TryBuild() => Post.Create(
        _id,
        _host,
        TimeRange.Create(_start, _end).Value,
        Coordinates.Create(52.52, 13.405).Value,
        new PostPlace("Venue", "Berlin", null, "Main St 1", "DE"),
        new EventDetails(
            EventType.HouseParty,
            PostTitle.Create("Garden party").Value,
            PostDescription.Create("Bring snacks and good vibes").Value,
            _requirements,
            Capacity.Create(20).Value,
            _entranceFee,
            TypeSpecificData: null),
        _pictures,
        _tags,
        Now);

    public Post Build() => TryBuild().Value;

    /// <summary>Today's wire shape of a built post, as the read model would project it (tag names are not joined).</summary>
    public PostDto BuildDto() => ToDto(Build());

    public static PostDto ToDto(Post post) => PostDto.Create(
        post.Id, post.PostTime, post.Schedule.Start, post.Schedule.End, post.Status, post.UserId, post.Location, post.Event, post.Pictures, []);

    /// <summary>Gives a picture the ID the database would assign when the post is saved.</summary>
    public static void AssignId(Picture picture, int id) =>
        typeof(Picture).GetProperty(nameof(Picture.Id), BindingFlags.Public | BindingFlags.Instance)!.SetValue(picture, PictureId.From(id));
}
