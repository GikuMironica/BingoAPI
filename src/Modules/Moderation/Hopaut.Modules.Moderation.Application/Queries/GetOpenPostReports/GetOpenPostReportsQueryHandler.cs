using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Queries.GetOpenPostReports;

public sealed class GetOpenPostReportsQueryHandler(IModerationRepository reports)
    : IQueryHandler<GetOpenPostReportsQuery, Result<IReadOnlyList<PostReportDto>>>
{
    public async ValueTask<Result<IReadOnlyList<PostReportDto>>> Handle(GetOpenPostReportsQuery request, CancellationToken cancellationToken)
    {
        var open = await reports.GetOpenPostReportsAsync(cancellationToken);
        IReadOnlyList<PostReportDto> dtos = open
            .Select(r => new PostReportDto(r.Id.Value, r.CreatedAt, r.Reason, r.Message, r.ReporterId.Value, r.ReportedHostId.Value, r.PostId.Value))
            .ToList();
        return Result.Success(dtos);
    }
}
