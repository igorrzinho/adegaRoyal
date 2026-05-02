namespace AdegaRoyal.Api.Entities;

/// <summary>
/// Represents a product category (e.g., Red Wine, White Wine, Beer).
/// </summary>
public class Category(Guid id, string? name, string? description = null)
{
    public Guid Id { get; } = id;
    public string Name { get; set; } = name ?? string.Empty;
    public string? Description { get; set; } = description;

    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
