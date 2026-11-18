using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Queries.GetAttendees;

public sealed class GetAttendeesQueryHandler(IAttendanceReadModel readModel, IPostAttendanceTermsQueries posts, IResourceAuthorizer authorizer)
    : IQueryHandler<GetAttendeesQuery, Result<AttendeePage>>
{
    public async ValueTask<Result<AttendeePage>> Handle(GetAttendeesQuery request, CancellationToken cancellationToken)
    {
        var status = AttendeeLists.ParseStatus(request.Status);
        var after = request.Cursor is null ? null : AttendeeKeyset.Decode(request.Cursor);
        var pageSize = request.Limit is { } limit ? PageSize.Create(limit) : Result.Success(PageSize.Default);

        var invalid = ValidationError.FromFailures(("status", status), ("cursor", after), ("limit", pageSize));
        if (invalid is not null)
            return invalid;

        var post = status.Value == AttendanceStatus.Pending
            ? await authorizer.GetTermsOfHostedPostAsync(posts, request.PostId, cancellationToken)
            : await posts.GetTermsAsync(request.PostId, cancellationToken);
        if (post.IsFailure)
            return post.Error;

        var attendees = await readModel.GetAttendeesAsync(
            request.PostId, status.Value, after?.Value, pageSize.Value.Value + 1, cancellationToken);

        var items = attendees.Take(pageSize.Value.Value).ToList();
        var nextCursor = attendees.Count > items.Count
            ? new AttendeeKeyset(items[^1].RequestedAt, items[^1].ParticipationId).Encode().Value
            : null;

        return new AttendeePage(items, nextCursor);
    }
}
