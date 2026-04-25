namespace KeycloakAuth.DTOs;

/// <summary>
/// Request DTO for updating an existing Product.
/// </summary>
public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
}
