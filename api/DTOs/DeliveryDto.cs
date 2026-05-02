namespace AdegaRoyal.Api.DTOs;

/// <summary>Represents a delivery record for an order.</summary>
public class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

/// <summary>Delivery DTO with verification code — only exposed to admins/couriers.</summary>
public class DeliveryAdminDto : DeliveryDto
{
    public string VerificationCode { get; set; } = string.Empty;
}

/// <summary>Payload for verifying the delivery OTP code.</summary>
public class VerifyDeliveryCodeDto
{
    public string Code { get; set; } = string.Empty;
}

/// <summary>Response from updating a delivery's status.</summary>
public class UpdateDeliveryStatusDto
{
    public string Status { get; set; } = string.Empty;
}
