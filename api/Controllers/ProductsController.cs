using KeycloakAuth.DTOs;
using KeycloakAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakAuth.Controllers;

/// <summary>
/// API endpoints for managing products in the catalog.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Get all products with optional category filtering.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts([FromQuery] Guid? categoryId)
    {
        if (categoryId.HasValue)
        {
            var products = await productService.GetProductsByCategoryAsync(categoryId.Value);
            return Ok(products);
        }

        var allProducts = await productService.GetAllProductsAsync();
        return Ok(allProducts);
    }

    /// <summary>
    /// Get a specific product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
    {
        var product = await productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    /// <summary>
    /// Create a new product (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing product (Admin only).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await productService.UpdateProductAsync(id, dto);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a product (Admin only).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var success = await productService.DeleteProductAsync(id);
        if (!success)
            return NotFound(new { message = "Product not found" });

        return NoContent();
    }
}
