namespace KeycloakAuth.DTOs;

/// <summary>Represents a shopping cart with its items.</summary>
public class CartDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
}

/// <summary>Represents a single item inside a cart.</summary>
public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
}

/// <summary>Payload to add a product to the cart or update its quantity.</summary>
public class AddCartItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
