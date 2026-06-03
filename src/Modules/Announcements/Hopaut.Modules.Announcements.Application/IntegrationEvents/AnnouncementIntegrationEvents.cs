using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application.IntegrationEvents;

public sealed record AnnouncementCreatedIntegrationEvent(AnnouncementId AnnouncementId, PostId PostId) : IntegrationEventBase;
