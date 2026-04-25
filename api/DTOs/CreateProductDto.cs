namespace KeycloakAuth.DTOs;

/// <summary>
/// Request DTO for creating a new Product.
/// </summary>
public class CreateProductDto
{
    public required string Name { get; set; } = string.Empty ;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
}
