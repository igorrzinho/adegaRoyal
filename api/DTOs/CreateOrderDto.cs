namespace AdegaRoyal.Api.DTOs;

/// <summary>
/// Request DTO for creating a new Order.
/// </summary>
public class CreateOrderDto
{
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
}
