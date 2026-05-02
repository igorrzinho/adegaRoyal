namespace AdegaRoyal.Api.Entities;

/// <summary>
/// Stores the BCrypt hash of a user's password.
/// Kept in a separate table so the password hash is never accidentally
/// exposed in queries that load the <see cref="User"/> aggregate.
/// </summary>
public class UserPassword
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Foreign key to <see cref="User"/>. One-to-one relationship.</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// BCrypt hash produced from the compound input: email + plainTextPassword.
    /// Never store or log the original value.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    // ── Navigation property ────────────────────────────────────────────────────
    public User User { get; set; } = null!;
}
