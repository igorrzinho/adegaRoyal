using BC = BCrypt.Net.BCrypt;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Implements password hashing using <b>BCrypt</b> (BCrypt.Net-Next, work-factor 12).
///
/// <para><b>Why BCrypt?</b><br/>
/// BCrypt is designed to be slow (adaptive cost factor), making brute-force and rainbow-table
/// attacks computationally expensive. It embeds the salt automatically in the stored hash,
/// so no separate salt column is needed.</para>
///
/// <para><b>Compound input strategy</b><br/>
/// The input hashed is <c>email.ToLowerInvariant() + plainPassword</c>.
/// This ties the hash to the email address, so a stolen hash from user A cannot be
/// replayed as user B's password even if they share the same plaintext password.
/// It acts as a natural, deterministic salt prefix on top of BCrypt's own random salt.</para>
/// </summary>
public class PasswordHasherService : IPasswordHasherService
{
    // Work factor 12 ≈ ~250 ms on modern hardware — good balance of security and UX.
    private const int WorkFactor = 12;

    /// <inheritdoc/>
    public string Hash(string email, string plainPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(plainPassword);

        var compound = BuildCompound(email, plainPassword);
        return BC.HashPassword(compound, workFactor: WorkFactor);
    }

    /// <inheritdoc/>
    public bool Verify(string email, string plainPassword, string hash)
    {
        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(plainPassword)
            || string.IsNullOrWhiteSpace(hash))
            return false;

        var compound = BuildCompound(email, plainPassword);
        return BC.Verify(compound, hash);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Normalises the email to lowercase before concatenation to ensure
    /// "User@Email.com" and "user@email.com" produce the same hash.
    /// </summary>
    private static string BuildCompound(string email, string plainPassword)
        => email.ToLowerInvariant() + plainPassword;
}
