using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain.Events;

public sealed record AnnouncementCreatedDomainEvent(AnnouncementId AnnouncementId, PostId PostId) : DomainEventBase;
