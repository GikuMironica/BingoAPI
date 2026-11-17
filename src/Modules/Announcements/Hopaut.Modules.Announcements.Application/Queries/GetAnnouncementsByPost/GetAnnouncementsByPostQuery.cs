using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;

public sealed record GetAnnouncementsByPostQuery(PostId PostId) : IQuery<Result<IReadOnlyList<AnnouncementDto>>>;

public sealed record AnnouncementDto(int Id, int PostId, string Message, DateTimeOffset CreatedAt);
