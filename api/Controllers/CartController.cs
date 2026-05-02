using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdegaRoyal.Api.Controllers;

/// <summary>
/// Manages the shopping cart for the currently authenticated user.
/// Customers can only access their own cart.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController(ICartService cartService) : ControllerBase
{
    /// <summary>Returns the current user's cart (creates one if it doesn't exist).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cart = await cartService.GetOrCreateCartAsync(userId);
        return Ok(cart);
    }

    /// <summary>
    /// Adds a product to the cart. If the product is already in the cart, increments its quantity.
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddCartItemDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var cart = await cartService.AddItemAsync(userId, dto);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Removes a specific item from the cart by CartItem ID.</summary>
    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid cartItemId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var cart = await cartService.RemoveItemAsync(userId, cartItemId);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Updates the quantity of a specific cart item. Setting quantity to 0 removes the item.</summary>
    [HttpPatch("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CartDto>> UpdateItemQuantity(Guid cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var cart = await cartService.UpdateItemQuantityAsync(userId, cartItemId, dto.Quantity);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Clears all items from the cart.</summary>
    [HttpDelete]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await cartService.ClearCartAsync(userId);
        return NoContent();
    }

    private string? GetUserId() =>
        User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

/// <summary>DTO for updating cart item quantity.</summary>
public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}
