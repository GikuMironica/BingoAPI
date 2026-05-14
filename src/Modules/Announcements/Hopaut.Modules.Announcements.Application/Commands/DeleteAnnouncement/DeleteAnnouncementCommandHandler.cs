using MediatR;

namespace Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;

public sealed class DeleteAnnouncementCommandHandler : IRequestHandler<DeleteAnnouncementCommand, bool>
{
    private readonly IAnnouncementRepository _repo;

    public DeleteAnnouncementCommandHandler(IAnnouncementRepository repo) => _repo = repo;

    public async Task<bool> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _repo.GetByIdAsync(request.AnnouncementId, cancellationToken);
        if (announcement is null) return false;

        await _repo.DeleteAsync(announcement, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
