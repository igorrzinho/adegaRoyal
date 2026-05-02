using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Enums;
using AdegaRoyal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdegaRoyal.Api.Controllers;

/// <summary>
/// Manages the order lifecycle.
/// - Customers: create (partial checkout), view own orders, cancel pending orders.
/// - Admins: view all orders, update any order status.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(
    IOrderService orderService,
    IPaymentService paymentService,
    IDeliveryService deliveryService) : ControllerBase
{
    // ─── Customer Endpoints ───────────────────────────────────────────────────

    /// <summary>Returns all orders belonging to the current authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var orders = await orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    /// <summary>Returns a specific order by ID. Customers can only see their own orders.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Admins can see any order
        OrderDto? order;
        if (User.IsInRole("Admin"))
            order = await orderService.GetOrderByIdAdminAsync(id);
        else
            order = await orderService.GetOrderByIdAsync(id, userId);

        return order == null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Partial checkout: converts selected CartItems into an Order and processes payment.
    /// Only the selected CartItem IDs are moved to the order; remaining items stay in the cart.
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(OrderDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(402)]
    public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!dto.CartItemIds.Any())
            return BadRequest(new { message = "Select at least one cart item to checkout." });

        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            // Step 1: Create the order from selected cart items
            var order = await orderService.CheckoutAsync(userId, dto);

            // Step 2: Process payment via Abacate Pay
            var paymentRequest = new PaymentRequestDto
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                CustomerEmail = userId, // Replace with actual email if available
                Description = $"Adega Royal Order #{order.Id}"
            };

            var paymentResult = await paymentService.ProcessPaymentAsync(paymentRequest);

            if (!paymentResult.Approved)
            {
                // Payment declined: cancel the order and restore stock
                await orderService.CancelOrderAsync(order.Id, userId);
                return StatusCode(402, new { message = paymentResult.Message });
            }

            // Step 3: Mark order as Paid
            var paidOrder = await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Paid);

            // Step 4: Auto-create delivery record with OTP
            await deliveryService.CreateDeliveryAsync(order.Id);

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, paidOrder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Cancels a pending order. Customers can only cancel their own orders.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var success = await orderService.CancelOrderAsync(id, userId);
            return success ? NoContent() : NotFound(new { message = "Order not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── Admin Endpoints ──────────────────────────────────────────────────────

    /// <summary>Returns all orders in the system (admin only).</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        var orders = await orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>Updates the status of any order (admin only).</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
            return BadRequest(new { message = "Invalid order status value." });

        try
        {
            var order = await orderService.UpdateOrderStatusAsync(id, status);
            return order == null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private string? GetUserId() =>
        User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

/// <summary>DTO for updating order status via PATCH.</summary>
public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}
