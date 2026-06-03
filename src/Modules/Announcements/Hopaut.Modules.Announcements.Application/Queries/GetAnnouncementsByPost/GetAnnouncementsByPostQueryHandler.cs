using MediatR;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;

public sealed class GetAnnouncementsByPostQueryHandler : IRequestHandler<GetAnnouncementsByPostQuery, IReadOnlyList<AnnouncementDto>>
{
    private readonly IAnnouncementRepository _repo;

    public GetAnnouncementsByPostQueryHandler(IAnnouncementRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<AnnouncementDto>> Handle(GetAnnouncementsByPostQuery request, CancellationToken cancellationToken)
    {
        var announcements = await _repo.GetByPostAsync(request.PostId, cancellationToken);
        return announcements.Select(a => new AnnouncementDto(a.Id.Value, a.PostId.Value, a.Message, a.CreatedAt)).ToList();
    }
}
