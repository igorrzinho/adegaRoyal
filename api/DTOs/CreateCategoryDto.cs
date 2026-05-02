namespace AdegaRoyal.Api.DTOs;

/// <summary>
/// Request DTO for creating a new Category.
/// </summary>
public class CreateCategoryDto
{
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
