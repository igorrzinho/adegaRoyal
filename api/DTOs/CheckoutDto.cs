using System.ComponentModel.DataAnnotations;

namespace KeycloakAuth.DTOs;

/// <summary>
/// Payload for partial checkout: customer selects which CartItem IDs to convert to an Order.
/// Remaining items stay in the cart.
/// </summary>
public class CheckoutDto
{
    /// <summary>The IDs of the CartItems to include in this order.</summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one cart item must be selected for checkout.")]
    public List<Guid> CartItemIds { get; set; } = new();

    /// <summary>Optional notes for the order (e.g., delivery instructions).</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
