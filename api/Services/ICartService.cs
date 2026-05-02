using AdegaRoyal.Api.DTOs;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Manages the shopping cart for a user.
/// Supports partial checkout: only selected CartItem IDs are checked out.
/// </summary>
public interface ICartService
{
    /// <summary>Returns the current cart for the given user (creates one if it doesn't exist).</summary>
    Task<CartDto> GetOrCreateCartAsync(string userId);

    /// <summary>Adds a product to the cart or increments its quantity if already present.</summary>
    Task<CartDto> AddItemAsync(string userId, AddCartItemDto dto);

    /// <summary>Removes a specific CartItem from the cart.</summary>
    Task<CartDto> RemoveItemAsync(string userId, Guid cartItemId);

    /// <summary>Updates the quantity of a specific CartItem. Quantity 0 removes the item.</summary>
    Task<CartDto> UpdateItemQuantityAsync(string userId, Guid cartItemId, int quantity);

    /// <summary>Clears all items from the cart.</summary>
    Task ClearCartAsync(string userId);
}
