using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;

public sealed record IsUserAttendingQuery(PostId PostId, UserId UserId) : IRequest<bool>;
