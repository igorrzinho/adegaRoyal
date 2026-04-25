using KeycloakAuth.DTOs;

namespace KeycloakAuth.Services;

/// <summary>
/// Abstraction for the Abacate Pay payment gateway.
/// Allows easy swapping of payment providers or test doubles.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Processes a payment request through Abacate Pay.
    /// Returns approval status, transaction ID and a descriptive message.
    /// </summary>
    Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto request);
}
