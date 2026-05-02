namespace AdegaRoyal.Api.DTOs;

/// <summary>
/// Response DTO for Order data (prevents circular references).
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}
