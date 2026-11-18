using System.Net.Http.Json;
using Hopaut.Modules.Payments.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hopaut.Modules.Payments.Infrastructure;

public sealed class PaymentsServiceOptions
{
    public string BaseUrl { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
}

public sealed class PaymentsServiceClient : IPaymentsServiceClient
{
    private readonly HttpClient _http;
    private readonly PaymentsServiceOptions _options;
    private readonly ILogger<PaymentsServiceClient> _logger;

    public PaymentsServiceClient(HttpClient http, IOptions<PaymentsServiceOptions> options, ILogger<PaymentsServiceClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePaymentAsync(string userId, decimal amount, string currency, string description, CancellationToken ct = default)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization = new("Bearer", _options.ApiKey);
            var response = await _http.PostAsJsonAsync($"{_options.BaseUrl}/payments",
                new { userId, amount, currency, description }, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>(ct);
            return new PaymentResult(true, PaymentId: result?.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for user {UserId}", userId);
            return new PaymentResult(false, Error: ex.Message);
        }
    }

    public async Task<PaymentStatus?> GetPaymentStatusAsync(string paymentId, CancellationToken ct = default)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization = new("Bearer", _options.ApiKey);
            var response = await _http.GetFromJsonAsync<PaymentStatus>($"{_options.BaseUrl}/payments/{paymentId}", ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment status for {PaymentId}", paymentId);
            return null;
        }
    }

    private sealed record CreatePaymentResponse(string PaymentId);
}
