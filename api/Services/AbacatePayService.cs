using KeycloakAuth.DTOs;
using Microsoft.Extensions.Logging;

namespace KeycloakAuth.Services;

/// <summary>
/// Simulated Abacate Pay integration.
/// In production, replace the simulation logic with real HTTP calls to the Abacate Pay REST API.
/// Uses primary constructor DI.
/// </summary>
public class AbacatePayService(ILogger<AbacatePayService> logger) : IPaymentService
{
    // Simulate a 90% approval rate for demo purposes
    private static readonly Random _random = new();

    public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto request)
    {
        logger.LogInformation(
            "Processing payment via Abacate Pay for Order {OrderId}, Amount: {Amount}",
            request.OrderId, request.Amount);

        // Simulate network latency (50–200ms)
        await Task.Delay(_random.Next(50, 200));

        // Simulate payment approval (90% success rate)
        var approved = _random.NextDouble() > 0.10;

        var response = new PaymentResponseDto
        {
            Approved = approved,
            TransactionId = approved ? $"ABCT-{Guid.NewGuid():N}"[..20] : string.Empty,
            Message = approved
                ? "Payment approved successfully."
                : "Payment declined by the issuing bank. Please try another card.",
            ProcessedAt = DateTime.UtcNow
        };

        logger.LogInformation(
            "Abacate Pay response for Order {OrderId}: Approved={Approved}, TransactionId={TransactionId}",
            request.OrderId, response.Approved, response.TransactionId);

        return response;
    }
}
