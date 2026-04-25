namespace KeycloakAuth.DTOs;

/// <summary>
/// Response DTO for OrderItem data (prevents circular references).
/// </summary>
public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductDto? Product { get; set; }
    public DateTime CreatedAt { get; set; }
}
