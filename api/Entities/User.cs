using AdegaRoyal.Api.Enums;

namespace AdegaRoyal.Api.Entities;

/// <summary>
/// Represents a registered user in the Adega Royal system.
/// </summary>
public class User(string name, string email, UserRole role = UserRole.Customer)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public string Email { get; set; } = email;
    public UserRole Role { get; set; } = role;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // ── Navigation properties ──────────────────────────────────────────────────
    public UserPassword? Password { get; set; }
    public ICollection<UserClaim> Claims { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
