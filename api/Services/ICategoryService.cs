using KeycloakAuth.DTOs;

namespace KeycloakAuth.Services;

/// <summary>
/// Service interface for managing product categories.
/// </summary>
public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(Guid id);
}
