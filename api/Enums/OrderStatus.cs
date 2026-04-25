namespace KeycloakAuth.Enums;

/// <summary>
/// Represents the lifecycle status of an order in the Adega Royal system.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order created, pending payment confirmation.</summary>
    Pending = 0,

    /// <summary>Payment confirmed, awaiting fulfillment.</summary>
    Paid = 1,

    /// <summary>Order packed and ready for shipping.</summary>
    Shipped = 2,

    /// <summary>Order delivered to customer.</summary>
    Delivered = 3,

    /// <summary>Order cancelled by customer or system.</summary>
    Cancelled = 4,

    /// <summary>Order returned for refund processing.</summary>
    Returned = 5
}
