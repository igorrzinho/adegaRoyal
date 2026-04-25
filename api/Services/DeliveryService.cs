using KeycloakAuth.Data;
using KeycloakAuth.DTOs;
using KeycloakAuth.Entities;
using KeycloakAuth.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeycloakAuth.Services;

/// <summary>
/// Delivery management service with OTP generation and verification.
/// Uses primary constructor for DI.
/// </summary>
public class DeliveryService(AppDbContext context, ILogger<DeliveryService> logger) : IDeliveryService
{
    private static readonly Random _random = new();

    public async Task<DeliveryAdminDto> CreateDeliveryAsync(Guid orderId)
    {
        // Ensure the order exists and is Paid
        var order = await context.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");

        if (order.Status != OrderStatus.Paid)
            throw new InvalidOperationException("A delivery can only be created for a paid order.");

        // Idempotency: return existing delivery if already created
        var existing = await context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
        if (existing != null)
            return MapToAdminDto(existing);

        // Generate 4-digit OTP
        var otp = GenerateOtp();
        logger.LogInformation("Generated OTP {Otp} for Order {OrderId}", otp, orderId);

        var delivery = new Delivery(Guid.NewGuid(), orderId, otp);
        context.Deliveries.Add(delivery);
        await context.SaveChangesAsync();

        return MapToAdminDto(delivery);
    }

    public async Task<DeliveryAdminDto?> GetDeliveryByOrderIdAdminAsync(Guid orderId)
    {
        var delivery = await context.Deliveries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

        return delivery == null ? null : MapToAdminDto(delivery);
    }

    public async Task<DeliveryDto?> GetDeliveryByOrderIdAsync(Guid orderId)
    {
        var delivery = await context.Deliveries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

        return delivery == null ? null : MapToDto(delivery);
    }

    public async Task<DeliveryDto?> UpdateStatusAsync(Guid orderId, DeliveryStatus status)
    {
        var delivery = await context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
        if (delivery == null) return null;

        delivery.Status = status;
        await context.SaveChangesAsync();
        return MapToDto(delivery);
    }

    public async Task<bool> VerifyDeliveryCodeAsync(Guid orderId, string code)
    {
        var delivery = await context.Deliveries
            .Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

        if (delivery == null)
            throw new InvalidOperationException("Delivery not found for this order.");

        if (delivery.Status == DeliveryStatus.Delivered)
            throw new InvalidOperationException("This order has already been delivered.");

        if (!string.Equals(delivery.VerificationCode, code.Trim(), StringComparison.Ordinal))
        {
            logger.LogWarning("Invalid delivery OTP attempt for Order {OrderId}", orderId);
            return false;
        }

        // Mark delivery as delivered
        delivery.Status = DeliveryStatus.Delivered;
        delivery.DeliveredAt = DateTime.UtcNow;

        // Mark order as delivered too
        if (delivery.Order != null)
            delivery.Order.Status = OrderStatus.Delivered;

        await context.SaveChangesAsync();

        logger.LogInformation("Order {OrderId} successfully delivered and confirmed via OTP.", orderId);
        return true;
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>Generates a zero-padded 4-digit OTP (e.g., "0042").</summary>
    private static string GenerateOtp() => _random.Next(0, 10000).ToString("D4");

    private static DeliveryAdminDto MapToAdminDto(Delivery d) => new()
    {
        Id = d.Id,
        OrderId = d.OrderId,
        Status = d.Status.ToString(),
        VerificationCode = d.VerificationCode,
        CreatedAt = d.CreatedAt,
        DeliveredAt = d.DeliveredAt
    };

    private static DeliveryDto MapToDto(Delivery d) => new()
    {
        Id = d.Id,
        OrderId = d.OrderId,
        Status = d.Status.ToString(),
        CreatedAt = d.CreatedAt,
        DeliveredAt = d.DeliveredAt
    };
}
