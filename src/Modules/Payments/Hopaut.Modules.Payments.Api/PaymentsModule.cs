using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Payments.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Payments.Api;

public sealed class PaymentsModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/payments")
            .WithTags("Payments")
            .RequireAuthorization();

        group.MapPost("/", async (CreatePaymentRequest request, HttpContext context, IPaymentsServiceClient client) =>
        {
            var userId = context.User.FindFirst("id")?.Value ?? "";
            var result = await client.CreatePaymentAsync(userId, request.Amount, request.Currency, request.Description);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapGet("/{paymentId}", async (string paymentId, IPaymentsServiceClient client) =>
        {
            var status = await client.GetPaymentStatusAsync(paymentId);
            return status is not null ? Results.Ok(status) : Results.NotFound();
        });
    }
}

public sealed record CreatePaymentRequest(decimal Amount, string Currency, string Description);
