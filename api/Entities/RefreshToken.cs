namespace AdegaRoyal.Api.Entities;

/// <summary>
/// Persists opaque refresh tokens so they can be validated, rotated, and revoked.
/// Each login issues a new token; old ones are replaced on refresh (token rotation).
/// </summary>
public class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Foreign key to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Cryptographically secure random string (Base64Url, 64 bytes).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>UTC expiry — typically 7-30 days.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC timestamp of creation.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>True after the token has been consumed or explicitly revoked.</summary>
    public bool IsRevoked { get; set; }

    // ── Navigation property ────────────────────────────────────────────────────
    public User User { get; set; } = null!;
}
