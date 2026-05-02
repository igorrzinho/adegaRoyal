namespace AdegaRoyal.Api.Entities;

/// <summary>
/// Represents a custom claim associated with a user (e.g., permissions, attributes).
/// Many claims can be assigned to a single user.
/// </summary>
public class UserClaim
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Foreign key to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The claim value (e.g., "can_manage_products", "vip_customer").
    /// The claim type is implicitly the system role; use ClaimValue for fine-grained permissions.
    /// </summary>
    public string ClaimValue { get; set; } = string.Empty;

    // ── Navigation property ────────────────────────────────────────────────────
    public User User { get; set; } = null!;
}
