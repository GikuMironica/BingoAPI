using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetPostById;

public sealed class GetPostByIdQueryHandler(IPostRepository posts) : IQueryHandler<GetPostByIdQuery, Result<PostDto>>
{
    public async ValueTask<Result<PostDto>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await posts.GetByIdAsync(request.PostId, cancellationToken);
        if (p is null)
            return PostErrors.NotFound(request.PostId);

        return new PostDto(
            p.Id.Value, p.PostTime, p.EventTime, p.EndTime, p.ActiveFlag, p.UserId.Value,
            new EventLocationDto(p.Location.EntityName, p.Location.City, p.Location.Region, p.Location.Address, p.Location.Country, p.Location.Location.X, p.Location.Location.Y),
            new EventDto(p.Event.EventType, p.Event.EntrancePrice, p.Event.Currency, p.Event.Title, p.Event.Description, p.Event.Requirements, p.Event.Slots),
            p.Pictures.Select(pic => new PictureDto(pic.Id.Value, pic.Url, pic.State.ToString())).ToList(),
            p.Tags.Select(t => t.Tag!.TagName).ToList());
    }
}
