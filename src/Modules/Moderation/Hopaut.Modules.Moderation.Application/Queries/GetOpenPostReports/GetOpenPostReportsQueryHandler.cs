using MediatR;

namespace Hopaut.Modules.Moderation.Application.Queries.GetOpenPostReports;

public sealed class GetOpenPostReportsQueryHandler : IRequestHandler<GetOpenPostReportsQuery, IReadOnlyList<PostReportDto>>
{
    private readonly IModerationRepository _repo;

    public GetOpenPostReportsQueryHandler(IModerationRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<PostReportDto>> Handle(GetOpenPostReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _repo.GetOpenPostReportsAsync(cancellationToken);
        return reports.Select(r => new PostReportDto(r.Id.Value, r.CreatedAt, r.Reason, r.Message, r.ReporterId.Value, r.ReportedHostId.Value, r.PostId.Value)).ToList();
    }
}
