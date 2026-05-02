namespace AdegaRoyal.Api.DTOs;

/// <summary>
/// Response DTO for Category data (prevents circular references).
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
