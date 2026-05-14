using Hopaut.Modules.Posts.Domain;
using MediatR;
using NetTopologySuite.Geometries;

namespace Hopaut.Modules.Posts.Application.Commands.CreatePost;

public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private readonly IPostRepository _repo;

    public CreatePostCommandHandler(IPostRepository repo) => _repo = repo;

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            PostTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventTime = request.EventTime,
            EndTime = request.EndTime,
            ActiveFlag = 1,
            UserId = request.UserId,
            Location = new EventLocation
            {
                EntityName = request.Location.EntityName,
                City = request.Location.City,
                Region = request.Location.Region,
                Address = request.Location.Address,
                Country = request.Location.Country,
                Location = new Point(request.Location.Longitude, request.Location.Latitude) { SRID = 4326 }
            },
            Event = new Event
            {
                EventType = request.Event.EventType,
                EntrancePrice = request.Event.EntrancePrice,
                Currency = request.Event.Currency,
                Title = request.Event.Title,
                Description = request.Event.Description,
                Requirements = request.Event.Requirements,
                Slots = request.Event.Slots,
                TypeSpecificData = request.Event.TypeSpecificData
            }
        };

        if (request.PictureUrls is { Count: > 0 })
        {
            post.Pictures = request.PictureUrls.Select(url => new Picture { Url = url }).ToList();
        }

        await _repo.AddAsync(post, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
