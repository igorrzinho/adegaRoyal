using KeycloakAuth.Data;
using KeycloakAuth.DTOs;
using KeycloakAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeycloakAuth.Services;

/// <summary>
/// Service implementation for managing product categories using primary constructor.
/// </summary>
public class CategoryService(AppDbContext context) : ICategoryService
{
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        return await context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null)
            return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category(Guid.NewGuid(), dto.Name, dto.Description);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto dto)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null)
            return null;

        category.Name = dto.Name;
        category.Description = dto.Description;
        await context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null)
            return false;

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}
