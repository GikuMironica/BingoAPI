using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, AnnouncementId>
{
    private readonly IAnnouncementRepository _repo;
    private readonly TimeProvider _timeProvider;

    public CreateAnnouncementCommandHandler(IAnnouncementRepository repo, TimeProvider timeProvider)
    {
        _repo = repo;
        _timeProvider = timeProvider;
    }

    public async Task<AnnouncementId> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = Announcement.Create(request.PostId, request.Message, _timeProvider.GetUtcNow());

        await _repo.AddAsync(announcement, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return announcement.Id;
    }
}
