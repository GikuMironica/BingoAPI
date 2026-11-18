using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain.Events;

/// <summary>The host (or an administrator) announced on a post; its accepted attendees are notified (plan Step 9.4).</summary>
public sealed record AnnouncementCreatedDomainEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : DomainEventBase(OccurredOnUtc);

/// <summary>The message of an announcement was replaced.</summary>
public sealed record AnnouncementEditedDomainEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : DomainEventBase(OccurredOnUtc);

/// <summary>An announcement was deleted.</summary>
public sealed record AnnouncementDeletedDomainEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : DomainEventBase(OccurredOnUtc);
