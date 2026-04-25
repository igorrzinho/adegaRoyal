namespace KeycloakAuth.DTOs;

/// <summary>
/// Request DTO for adding items to an Order.
/// </summary>
public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
