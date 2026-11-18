using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Contracts;

/// <summary>The host (or an administrator) announced on a post; its accepted attendees are notified (plan Step 9.4).</summary>
public sealed record AnnouncementCreatedIntegrationEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : IntegrationEventBase(OccurredOnUtc);

/// <summary>The message of an announcement was replaced.</summary>
public sealed record AnnouncementEditedIntegrationEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : IntegrationEventBase(OccurredOnUtc);

/// <summary>An announcement was deleted.</summary>
public sealed record AnnouncementDeletedIntegrationEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : IntegrationEventBase(OccurredOnUtc);
