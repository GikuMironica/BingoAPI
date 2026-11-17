using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;

public sealed class GetAnnouncementsByPostQueryHandler(IAnnouncementRepository announcements)
    : IQueryHandler<GetAnnouncementsByPostQuery, Result<IReadOnlyList<AnnouncementDto>>>
{
    public async ValueTask<Result<IReadOnlyList<AnnouncementDto>>> Handle(GetAnnouncementsByPostQuery request, CancellationToken cancellationToken)
    {
        var found = await announcements.GetByPostAsync(request.PostId, cancellationToken);
        IReadOnlyList<AnnouncementDto> dtos = found
            .Select(a => new AnnouncementDto(a.Id.Value, a.PostId.Value, a.Message, a.CreatedAt))
            .ToList();
        return Result.Success(dtos);
    }
}
