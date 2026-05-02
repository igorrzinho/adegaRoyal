using System.ComponentModel.DataAnnotations;

namespace AdegaRoyal.Api.DTOs;

/// <summary>Payload for authenticating a user via Keycloak (returns JWT).</summary>
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Response returned upon successful authentication.</summary>
public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}
