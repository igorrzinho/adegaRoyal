using KeycloakAuth.DTOs;
using KeycloakAuth.Enums;

namespace KeycloakAuth.Services;

/// <summary>
/// Manages order lifecycle including partial checkout from cart items.
/// </summary>
public interface IOrderService
{
    /// <summary>Gets an order by ID. Customers see only their own orders.</summary>
    Task<OrderDto?> GetOrderByIdAsync(Guid id, string userId);

    /// <summary>Gets an order by ID ignoring ownership (admin use).</summary>
    Task<OrderDto?> GetOrderByIdAdminAsync(Guid id);

    /// <summary>Gets all orders for a specific user.</summary>
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);

    /// <summary>Gets all orders in the system (admin only).</summary>
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

    /// <summary>
    /// Partial checkout: converts selected CartItems into an Order.
    /// Non-selected items remain in the cart.
    /// </summary>
    Task<OrderDto> CheckoutAsync(string userId, CheckoutDto dto);

    /// <summary>Updates the status of any order (admin only).</summary>
    Task<OrderDto?> UpdateOrderStatusAsync(Guid id, OrderStatus status);

    /// <summary>Cancels a pending order (customer can only cancel their own).</summary>
    Task<bool> CancelOrderAsync(Guid id, string userId);
}
