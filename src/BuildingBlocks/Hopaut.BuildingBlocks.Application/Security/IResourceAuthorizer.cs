namespace Hopaut.BuildingBlocks.Application.Security;

/// <summary>
/// Resource-based authorization port (plan Step 6.9). A handler loads the resource, then asks whether the caller satisfies
/// a named policy for it; modules register their policies in their Infrastructure. Callers outside a request (background
/// work) are anonymous, so ownership policies deny them.
/// </summary>
public interface IResourceAuthorizer
{
    Task<bool> IsAuthorizedAsync(object resource, string policy, CancellationToken cancellationToken);
}
