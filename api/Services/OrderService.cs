using AdegaRoyal.Api.Data;
using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Entities;
using AdegaRoyal.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Order management service supporting partial checkout from cart items.
/// Uses primary constructor for DI.
/// </summary>
public class OrderService(AppDbContext context, IProductService productService) : IOrderService
{
    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, string userId)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && o.UserId == userId)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync();

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAdminAsync(Guid id)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync();

        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    /// <summary>
    /// Partial checkout flow:
    /// 1. Load selected CartItems from the user's cart.
    /// 2. Validate stock for each selected item.
    /// 3. Create the Order + OrderItems.
    /// 4. Deduct stock.
    /// 5. Remove only the checked-out CartItems (others remain in cart).
    /// </summary>
    public async Task<OrderDto> CheckoutAsync(string userId, CheckoutDto dto)
    {
        // Load cart
        var cart = await context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new InvalidOperationException("Cart not found. Add items to your cart first.");

        // Filter to requested CartItems
        var selectedItems = cart.Items
            .Where(i => dto.CartItemIds.Contains(i.Id))
            .ToList();

        if (selectedItems.Count == 0)
            throw new InvalidOperationException("None of the provided cart item IDs were found in your cart.");

        if (selectedItems.Count != dto.CartItemIds.Count)
            throw new InvalidOperationException("One or more cart item IDs do not belong to your cart.");

        // Validate stock
        foreach (var item in selectedItems)
        {
            if (item.Product == null)
                throw new InvalidOperationException($"Product data missing for cart item {item.Id}.");

            var hasStock = await productService.HasSufficientStockAsync(item.ProductId, item.Quantity);
            if (!hasStock)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{item.Product.Name}'. Available: {item.Product.StockQuantity}.");
        }

        // Calculate total
        var totalAmount = selectedItems.Sum(i => (i.Product?.Price ?? 0m) * i.Quantity);

        // Create Order
        var order = new Order(Guid.NewGuid(), userId, totalAmount, OrderStatus.Pending)
        {
            Notes = dto.Notes
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Create OrderItems and deduct stock
        foreach (var item in selectedItems)
        {
            var orderItem = new OrderItem(
                Guid.NewGuid(),
                order.Id,
                item.ProductId,
                item.Quantity,
                item.Product!.Price
            );
            context.OrderItems.Add(orderItem);
            await productService.DeductStockAsync(item.ProductId, item.Quantity);
        }

        // Remove only the checked-out CartItems
        context.CartItems.RemoveRange(selectedItems);
        cart.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // Reload with navigation data
        var created = await context.Orders
            .AsNoTracking()
            .Where(o => o.Id == order.Id)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstAsync();

        return MapToDto(created);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(Guid id, OrderStatus status)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;

        order.Status = status;
        await context.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<bool> CancelOrderAsync(Guid id, string userId)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null) return false;

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be cancelled.");

        // Restore stock
        foreach (var item in order.OrderItems)
            await productService.RestoreStockAsync(item.ProductId, item.Quantity);

        order.Status = OrderStatus.Cancelled;
        await context.SaveChangesAsync();
        return true;
    }

    // ─── Mapper ───────────────────────────────────────────────────────────────

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        CreatedAt = order.CreatedAt,
        TotalAmount = order.TotalAmount,
        Status = order.Status.ToString(),
        Notes = order.Notes,
        OrderItems = order.OrderItems.Select(oi => new OrderItemDto
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
            CreatedAt = oi.CreatedAt,
            Product = oi.Product == null ? null : new ProductDto
            {
                Id = oi.Product.Id,
                Name = oi.Product.Name,
                Description = oi.Product.Description,
                Price = oi.Product.Price,
                StockQuantity = oi.Product.StockQuantity,
                ImageUrl = oi.Product.ImageUrl,
                CategoryId = oi.Product.CategoryId,
                CreatedAt = oi.Product.CreatedAt,
                UpdatedAt = oi.Product.UpdatedAt
            }
        }).ToList()
    };
}
