using AdegaRoyal.Api.Data;
using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Service implementation for managing products using primary constructor.
/// Handles catalog operations and stock validation.
/// </summary>
public class ProductService(AppDbContext context) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        return await context.Products
            .Include(p => p.Category)
            .Select(p => MapProductToDto(p))
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
    {
        return await context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Select(p => MapProductToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : MapProductToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product(
            Guid.NewGuid(),
            dto.Name,
            dto.Description,
            dto.Price,
            dto.StockQuantity,
            dto.ImageUrl,
            dto.CategoryId
        );

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Reload to get the category
        await context.Entry(product).Reference(p => p.Category).LoadAsync();

        return MapProductToDto(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            product.Name = dto.Name;
        if (dto.Description != null)
            product.Description = dto.Description;
        if (dto.Price.HasValue)
            product.Price = dto.Price.Value;
        if (dto.StockQuantity.HasValue)
            product.StockQuantity = dto.StockQuantity.Value;
        if (dto.ImageUrl != null)
            product.ImageUrl = dto.ImageUrl;
        if (dto.CategoryId.HasValue)
            product.CategoryId = dto.CategoryId.Value;

        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return MapProductToDto(product);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null)
            return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasSufficientStockAsync(Guid productId, int requestedQuantity)
    {
        var product = await context.Products.FindAsync(productId);
        return product != null && product.StockQuantity >= requestedQuantity;
    }

    public async Task<bool> DeductStockAsync(Guid productId, int quantity)
    {
        var product = await context.Products.FindAsync(productId);
        if (product == null || product.StockQuantity < quantity)
            return false;

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task RestoreStockAsync(Guid productId, int quantity)
    {
        var product = await context.Products.FindAsync(productId);
        if (product == null) return;

        product.StockQuantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    private static ProductDto MapProductToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            Category = product.Category == null ? null : new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Description = product.Category.Description
            },
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
