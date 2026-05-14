namespace Hopaut.Modules.Payments.Application;

/// <summary>
/// Thin proxy client for the external payments service.
/// </summary>
public interface IPaymentsServiceClient
{
    Task<PaymentResult> CreatePaymentAsync(string userId, decimal amount, string currency, string description, CancellationToken ct = default);
    Task<PaymentStatus?> GetPaymentStatusAsync(string paymentId, CancellationToken ct = default);
}

public sealed record PaymentResult(bool Success, string? PaymentId = null, string? Error = null);
public sealed record PaymentStatus(string PaymentId, string Status, decimal Amount, string Currency);
