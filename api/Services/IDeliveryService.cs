using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Enums;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Manages delivery records, OTP generation and verification.
/// </summary>
public interface IDeliveryService
{
    /// <summary>
    /// Creates a delivery record for a paid order.
    /// Generates a random 4-digit OTP verification code.
    /// </summary>
    Task<DeliveryAdminDto> CreateDeliveryAsync(Guid orderId);

    /// <summary>Returns the delivery record for a given order (admin view with code).</summary>
    Task<DeliveryAdminDto?> GetDeliveryByOrderIdAdminAsync(Guid orderId);

    /// <summary>Returns the delivery record for a given order (customer view without code).</summary>
    Task<DeliveryDto?> GetDeliveryByOrderIdAsync(Guid orderId);

    /// <summary>Updates the delivery status (e.g., Preparing → WaitingForCourier → OnTheWay).</summary>
    Task<DeliveryDto?> UpdateStatusAsync(Guid orderId, DeliveryStatus status);

    /// <summary>
    /// Verifies the OTP code provided by the customer at delivery.
    /// If correct, marks delivery as Delivered and the associated Order as Delivered.
    /// </summary>
    Task<bool> VerifyDeliveryCodeAsync(Guid orderId, string code);
}
