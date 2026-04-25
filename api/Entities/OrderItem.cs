namespace KeycloakAuth.Entities;

/// <summary>
/// Represents a line item within an order (specific product and quantity).
/// </summary>
public class OrderItem(Guid id, Guid orderId, Guid productId, int quantity, decimal unitPrice)
{
    public Guid Id { get; } = id;
    public Guid OrderId { get; set; } = orderId;
    public Guid ProductId { get; set; } = productId;
    public int Quantity { get; set; } = quantity;
    public decimal UnitPrice { get; set; } = unitPrice;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
