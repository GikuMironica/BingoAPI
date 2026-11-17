using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Contracts;

public sealed record AnnouncementCreatedIntegrationEvent(DateTimeOffset OccurredOnUtc, AnnouncementId AnnouncementId, PostId PostId) : IntegrationEventBase(OccurredOnUtc);
