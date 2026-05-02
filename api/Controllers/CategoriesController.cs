using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdegaRoyal.Api.Controllers;

/// <summary>
/// API endpoints for managing product categories.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>
    /// Get all product categories.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get a specific category by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }

    /// <summary>
    /// Create a new category (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Update an existing category (Admin only).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await categoryService.UpdateCategoryAsync(id, dto);
        if (category == null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }

    /// <summary>
    /// Delete a category (Admin only).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var success = await categoryService.DeleteCategoryAsync(id);
        if (!success)
            return NotFound(new { message = "Category not found" });

        return NoContent();
    }
}
