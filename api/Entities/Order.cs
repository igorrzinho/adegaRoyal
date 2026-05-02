using AdegaRoyal.Api.Enums;

namespace AdegaRoyal.Api.Entities;

/// <summary>
/// Represents a customer order in the Adega Royal system.
/// Supports partial checkout: created from a subset of CartItems.
/// </summary>
public class Order(Guid id, string userId, decimal totalAmount, OrderStatus status = OrderStatus.Pending)
{
    public Guid Id { get; init; } = id;
    public string UserId { get; set; } = userId;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; } = totalAmount;
    public OrderStatus Status { get; set; } = status;
    public string? Notes { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Delivery? Delivery { get; set; }
}
