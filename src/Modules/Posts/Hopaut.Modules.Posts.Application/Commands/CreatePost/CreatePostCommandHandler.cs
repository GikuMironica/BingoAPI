using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using MediatR;
using NetTopologySuite.Geometries;

namespace Hopaut.Modules.Posts.Application.Commands.CreatePost;

public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostId>
{
    private readonly IPostRepository _repo;

    public CreatePostCommandHandler(IPostRepository repo) => _repo = repo;

    public async Task<PostId> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = Post.Create(
            request.UserId,
            request.EventTime,
            request.EndTime,
            new EventLocation
            {
                EntityName = request.Location.EntityName,
                City = request.Location.City,
                Region = request.Location.Region,
                Address = request.Location.Address,
                Country = request.Location.Country,
                Location = new Point(request.Location.Longitude, request.Location.Latitude) { SRID = 4326 }
            },
            new Event
            {
                EventType = request.Event.EventType,
                EntrancePrice = request.Event.EntrancePrice,
                Currency = request.Event.Currency,
                Title = request.Event.Title,
                Description = request.Event.Description,
                Requirements = request.Event.Requirements,
                Slots = request.Event.Slots,
                TypeSpecificData = request.Event.TypeSpecificData
            },
            request.PictureUrls?.Select(url => new Picture { Url = url }).ToList()
        );

        await _repo.AddAsync(post, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
