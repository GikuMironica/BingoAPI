using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Payments.Api;

/// <summary>Maps the Payments module's endpoints; its services are registered by <c>AddPaymentsModule</c> in the module's Infrastructure.</summary>
public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new PaymentsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
