using AdegaRoyal.Api.DTOs;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Service interface for managing products in the catalog.
/// </summary>
public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(Guid id);
    Task<bool> HasSufficientStockAsync(Guid productId, int requestedQuantity);
    Task<bool> DeductStockAsync(Guid productId, int quantity);

    /// <summary>Restores stock units when an order is cancelled.</summary>
    Task RestoreStockAsync(Guid productId, int quantity);
}
