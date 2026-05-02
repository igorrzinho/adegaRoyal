using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Generates signed JWT access tokens and cryptographically secure refresh tokens.
///
/// <para><b>Access Token payload</b><br/>
/// <list type="bullet">
///   <item><c>sub</c> — user database ID (Guid)</item>
///   <item><c>name</c> — display name</item>
///   <item><c>email</c> — normalized email</item>
///   <item><c>role</c> — user role (maps to <see cref="ClaimTypes.Role"/>)</item>
///   <item>one <c>permission</c> claim per <c>UserClaim.ClaimValue</c></item>
/// </list>
/// </para>
///
/// <para><b>Refresh Token</b><br/>
/// A 64-byte cryptographically random value encoded as Base64Url.
/// It is stored in the <c>RefreshTokens</c> table and validated server-side
/// (stateful) — the token itself carries no information.</para>
///
/// <para><b>Configuration keys expected in appsettings.json</b><br/>
/// <code>
/// "Jwt": {
///   "SecretKey": "at-least-32-character-secret-key",
///   "Issuer":    "AdegaRoyal",
///   "Audience":  "AdegaRoyalClient",
///   "AccessTokenExpirationMinutes": 15,
///   "RefreshTokenExpirationDays":   7
/// }
/// </code>
/// </para>
/// </summary>
public class TokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public int AccessTokenExpiresInSeconds => _accessTokenExpirationMinutes * 60;
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_refreshTokenExpirationDays);

    public TokenService(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");

        _secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured. Add 'Jwt:SecretKey' to appsettings.");

        if (_secretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long.");

        _issuer = jwtSection["Issuer"] ?? "AdegaRoyal";
        _audience = jwtSection["Audience"] ?? "AdegaRoyalClient";
        _accessTokenExpirationMinutes = int.TryParse(jwtSection["AccessTokenExpirationMinutes"], out var atMin) ? atMin : 15;
        _refreshTokenExpirationDays = int.TryParse(jwtSection["RefreshTokenExpirationDays"], out var rtDays) ? rtDays : 7;
    }

    /// <inheritdoc/>
    public string GenerateAccessToken(
        Guid userId,
        string name,
        string email,
        string role,
        IEnumerable<string> claimValues)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,   DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Name,  name),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role,               role),
        };

        // Add each UserClaim as a "permission" claim in the token payload
        foreach (var claimValue in claimValues)
            claims.Add(new Claim("permission", claimValue));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('='); 
    }
}
