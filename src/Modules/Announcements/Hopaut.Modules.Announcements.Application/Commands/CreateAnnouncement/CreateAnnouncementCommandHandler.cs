using Hopaut.Modules.Announcements.Domain;
using MediatR;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, int>
{
    private readonly IAnnouncementRepository _repo;

    public CreateAnnouncementCommandHandler(IAnnouncementRepository repo) => _repo = repo;

    public async Task<int> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = new Announcement
        {
            PostId = request.PostId,
            Message = request.Message,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        await _repo.AddAsync(announcement, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return announcement.Id;
    }
}
