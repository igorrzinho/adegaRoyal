namespace AdegaRoyal.Api.DTOs;

/// <summary>
/// Response DTO for Product data (prevents circular references).
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
