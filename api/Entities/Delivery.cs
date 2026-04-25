using KeycloakAuth.Enums;

namespace KeycloakAuth.Entities;

/// <summary>
/// Represents a delivery record tied to a specific order.
/// Uses OTP verification to confirm delivery to the customer.
/// </summary>
public class Delivery(Guid id, Guid orderId, string verificationCode)
{
    public Guid Id { get; init; } = id;
    public Guid OrderId { get; set; } = orderId;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Preparing;

    /// <summary>4-digit OTP code generated at delivery creation.</summary>
    public string VerificationCode { get; set; } = verificationCode;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }

    // Navigation property
    public Order? Order { get; set; }
}
