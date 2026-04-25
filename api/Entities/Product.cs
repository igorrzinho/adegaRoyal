namespace KeycloakAuth.Entities;

/// <summary>
/// Represents a product in the Adega Royal catalog (wine, spirits, beer, etc.).
/// </summary>
public class Product(
    Guid id,
    string name,
    string? description,
    decimal price,
    int stockQuantity,
    string? imageUrl,
    Guid categoryId)
{
    public Guid Id { get; init; } = id;
    public string Name { get; set; } = name ?? string.Empty;
    public string? Description { get; set; } = description;
    public decimal Price { get; set; } = price;
    public int StockQuantity { get; set; } = stockQuantity;
    public string? ImageUrl { get; set; } = imageUrl;
    public Guid CategoryId { get; set; } = categoryId;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
