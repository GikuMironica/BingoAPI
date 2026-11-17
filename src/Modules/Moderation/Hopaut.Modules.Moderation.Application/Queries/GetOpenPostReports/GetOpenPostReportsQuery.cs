using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Queries.GetOpenPostReports;

public sealed record GetOpenPostReportsQuery() : IQuery<Result<IReadOnlyList<PostReportDto>>>;

public sealed record PostReportDto(int Id, DateTimeOffset CreatedAt, int Reason, string? Message, string ReporterId, string ReportedHostId, int PostId);
