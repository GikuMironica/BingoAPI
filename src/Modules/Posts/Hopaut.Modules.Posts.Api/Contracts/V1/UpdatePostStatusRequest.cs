namespace Hopaut.Modules.Posts.Api.Contracts.V1;

/// <summary>Body of <c>PATCH /api/v1/posts/{postId}/status</c> (administrators): the target status; only <c>"disabled"</c> exists today.</summary>
public sealed record UpdatePostStatusRequest(string? Status);
