namespace KeycloakAuth.Enums;

/// <summary>
/// Represents the lifecycle status of a delivery in the Adega Royal system.
/// </summary>
public enum DeliveryStatus
{
    /// <summary>Order is being packed and prepared at the store.</summary>
    Preparing = 0,

    /// <summary>Order is ready and waiting for a courier to pick it up.</summary>
    WaitingForCourier = 1,

    /// <summary>Courier has picked up the order and is en route to the customer.</summary>
    OnTheWay = 2,

    /// <summary>Order has been delivered and confirmed with the OTP code.</summary>
    Delivered = 3
}
