namespace KeycloakAuth.Entities;

/// <summary>
/// Represents a shopping cart belonging to a single user.
/// One cart per user — items persist until checkout or removal.
/// </summary>
public class Cart(Guid id, string userId)
{
    public Guid Id { get; init; } = id;
    public string UserId { get; set; } = userId;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
