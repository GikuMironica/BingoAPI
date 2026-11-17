using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain.Events;

public sealed record AnnouncementCreatedDomainEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : DomainEventBase(OccurredOnUtc);
