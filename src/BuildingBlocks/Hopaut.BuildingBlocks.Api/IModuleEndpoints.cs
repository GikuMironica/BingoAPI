using Microsoft.AspNetCore.Routing;

namespace Hopaut.BuildingBlocks.Api;

/// <summary>
/// Marker interface for module endpoint registration via minimal APIs.
/// Each module implements this to register its endpoints.
/// </summary>
public interface IModuleEndpoints
{
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
