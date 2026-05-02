using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Enums;
using AdegaRoyal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdegaRoyal.Api.Controllers;

/// <summary>
/// Manages delivery records and OTP-based delivery confirmation.
/// - Admins/Couriers: create, update status, view code, verify code.
/// - Customers: view delivery status (without the OTP code).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveriesController(IDeliveryService deliveryService) : ControllerBase
{
    /// <summary>
    /// Returns delivery info for an order.
    /// Admins receive the OTP code; customers see status only.
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(DeliveryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDeliveryByOrderId(Guid orderId)
    {
        if (User.IsInRole("Admin"))
        {
            var adminDto = await deliveryService.GetDeliveryByOrderIdAdminAsync(orderId);
            return adminDto == null ? NotFound() : Ok(adminDto);
        }

        var dto = await deliveryService.GetDeliveryByOrderIdAsync(orderId);
        return dto == null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Creates a delivery record for a paid order and generates a 4-digit OTP code.
    /// Admin only — typically called automatically after checkout.
    /// </summary>
    [HttpPost("order/{orderId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DeliveryAdminDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DeliveryAdminDto>> CreateDelivery(Guid orderId)
    {
        try
        {
            var delivery = await deliveryService.CreateDeliveryAsync(orderId);
            return CreatedAtAction(nameof(GetDeliveryByOrderId), new { orderId }, delivery);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the delivery status (e.g., Preparing → WaitingForCourier → OnTheWay).
    /// Admin only.
    /// </summary>
    [HttpPatch("order/{orderId:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DeliveryDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DeliveryDto>> UpdateStatus(Guid orderId, [FromBody] UpdateDeliveryStatusDto dto)
    {
        if (!Enum.TryParse<DeliveryStatus>(dto.Status, true, out var status))
            return BadRequest(new { message = "Invalid delivery status. Valid values: Preparing, WaitingForCourier, OnTheWay, Delivered." });

        var result = await deliveryService.UpdateStatusAsync(orderId, status);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Verifies the 4-digit OTP code provided by the customer at delivery.
    /// If the code matches, marks the delivery and the associated order as Delivered.
    /// </summary>
    [HttpPost("order/{orderId:guid}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> VerifyDeliveryCode(Guid orderId, [FromBody] VerifyDeliveryCodeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Verification code is required." });

        try
        {
            var verified = await deliveryService.VerifyDeliveryCodeAsync(orderId, dto.Code);

            return verified
                ? Ok(new { message = "Delivery confirmed successfully. Order marked as Delivered." })
                : BadRequest(new { message = "Invalid verification code. Please try again." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
