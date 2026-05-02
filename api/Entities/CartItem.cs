namespace AdegaRoyal.Api.Entities;

/// <summary>
/// Represents a single product line item within a shopping cart.
/// Supports partial checkout: only selected CartItems are converted to OrderItems.
/// </summary>
public class CartItem(Guid id, Guid cartId, Guid productId, int quantity)
{
    public Guid Id { get; init; } = id;
    public Guid CartId { get; set; } = cartId;
    public Guid ProductId { get; set; } = productId;
    public int Quantity { get; set; } = quantity;
    public DateTime AddedAt { get; init; } = DateTime.UtcNow;

    // Navigation properties
    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
}
