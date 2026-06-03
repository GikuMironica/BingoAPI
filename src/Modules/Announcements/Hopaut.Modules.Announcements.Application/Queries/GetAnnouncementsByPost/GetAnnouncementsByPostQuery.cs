using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;

public sealed record GetAnnouncementsByPostQuery(PostId PostId) : IRequest<IReadOnlyList<AnnouncementDto>>;

public sealed record AnnouncementDto(int Id, int PostId, string Message, DateTimeOffset CreatedAt);
