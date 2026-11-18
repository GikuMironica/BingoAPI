using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementById;

/// <summary>One announcement, for the post's host, its accepted attendees and administrators (decision D-033).</summary>
public sealed record GetAnnouncementByIdQuery(AnnouncementId AnnouncementId) : IQuery<Result<AnnouncementDto>>;
