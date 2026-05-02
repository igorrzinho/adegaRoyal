using AdegaRoyal.Api.Data;
using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Cart management service using primary constructor DI.
/// Handles add/remove/update operations and maps results to DTOs.
/// </summary>
public class CartService(AppDbContext context) : ICartService
{
    public async Task<CartDto> GetOrCreateCartAsync(string userId)
    {
        var cart = await GetCartWithItemsAsync(userId);

        if (cart == null)
        {
            cart = new Cart(Guid.NewGuid(), userId);
            context.Carts.Add(cart);
            await context.SaveChangesAsync();
        }

        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddCartItemDto dto)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        // Validate product exists and has stock
        var product = await context.Products.FindAsync(dto.ProductId)
            ?? throw new InvalidOperationException($"Product {dto.ProductId} not found.");

        if (!product.IsActive)
            throw new InvalidOperationException("Product is not available.");

        if (product.StockQuantity < dto.Quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}.");

        var cart = await GetCartWithItemsAsync(userId);
        if (cart == null)
        {
            cart = new Cart(Guid.NewGuid(), userId);
            context.Carts.Add(cart);
            await context.SaveChangesAsync();
        }

        // Check if product is already in cart
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
        }
        else
        {
            var newItem = new CartItem(Guid.NewGuid(), cart.Id, dto.ProductId, dto.Quantity);
            context.CartItems.Add(newItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Reload to get product data
        var updatedCart = await GetCartWithItemsAsync(userId);
        return MapToDto(updatedCart!);
    }

    public async Task<CartDto> RemoveItemAsync(string userId, Guid cartItemId)
    {
        var cart = await GetCartWithItemsAsync(userId)
            ?? throw new InvalidOperationException("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new InvalidOperationException("Cart item not found.");

        context.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var updatedCart = await GetCartWithItemsAsync(userId);
        return MapToDto(updatedCart!);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(string userId, Guid cartItemId, int quantity)
    {
        var cart = await GetCartWithItemsAsync(userId)
            ?? throw new InvalidOperationException("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new InvalidOperationException("Cart item not found.");

        if (quantity <= 0)
        {
            context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var updatedCart = await GetCartWithItemsAsync(userId);
        return MapToDto(updatedCart!);
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetCartWithItemsAsync(userId);
        if (cart == null) return;

        context.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private async Task<Cart?> GetCartWithItemsAsync(string userId)
    {
        return await context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private static CartDto MapToDto(Cart cart) => new()
    {
        Id = cart.Id,
        UserId = cart.UserId,
        UpdatedAt = cart.UpdatedAt,
        Items = cart.Items.Select(i => new CartItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            ProductImageUrl = i.Product?.ImageUrl,
            UnitPrice = i.Product?.Price ?? 0m,
            Quantity = i.Quantity,
            AddedAt = i.AddedAt
        }).ToList()
    };
}
