namespace AdegaRoyal.Api.Services;

/// <summary>
/// Contract for hashing and verifying passwords using the compound input (email + password).
/// </summary>
public interface IPasswordHasherService
{
    /// <summary>
    /// Generates a BCrypt hash from the compound input <c>email.ToLower() + plainPassword</c>.
    /// </summary>
    string Hash(string email, string plainPassword);

    /// <summary>
    /// Verifies whether <paramref name="plainPassword"/> matches the stored <paramref name="hash"/>
    /// using the same compound strategy.
    /// </summary>
    bool Verify(string email, string plainPassword, string hash);
}
