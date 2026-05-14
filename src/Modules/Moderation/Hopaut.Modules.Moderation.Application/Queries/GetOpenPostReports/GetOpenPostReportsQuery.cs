using MediatR;

namespace Hopaut.Modules.Moderation.Application.Queries.GetOpenPostReports;

public sealed record GetOpenPostReportsQuery() : IRequest<IReadOnlyList<PostReportDto>>;

public sealed record PostReportDto(int Id, long Timestamp, int Reason, string? Message, string ReporterId, string ReportedHostId, int PostId);
