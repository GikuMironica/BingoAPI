using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;

/// <summary>
/// Every announcement of a post, newest first, for the post's host, its accepted attendees and administrators (decision
/// D-033). Unpaged like today's route; plan Step 10.6 replaces it with <c>GetPostAnnouncementsQuery</c>.
/// </summary>
public sealed record GetAnnouncementsByPostQuery(PostId PostId) : IQuery<Result<IReadOnlyList<AnnouncementDto>>>;
