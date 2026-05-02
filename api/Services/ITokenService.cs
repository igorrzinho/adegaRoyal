namespace AdegaRoyal.Api.Services;

/// <summary>
/// Contract for generating JWT access tokens and opaque refresh tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    /// <param name="userId">Database primary key.</param>
    /// <param name="name">User display name (included in token payload).</param>
    /// <param name="email">User email address.</param>
    /// <param name="role">User role string (e.g. "Admin", "Customer").</param>
    /// <param name="claimValues">Additional custom claims from <c>UserClaim</c> table.</param>
    /// <returns>Signed JWT string.</returns>
    string GenerateAccessToken(Guid userId, string name, string email, string role, IEnumerable<string> claimValues);

    /// <summary>
    /// Generates a cryptographically secure opaque refresh token (Base64Url, 64 bytes).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>Lifetime of the access token in seconds (e.g. 900 = 15 min).</summary>
    int AccessTokenExpiresInSeconds { get; }

    /// <summary>Lifetime of the refresh token.</summary>
    TimeSpan RefreshTokenLifetime { get; }
}
