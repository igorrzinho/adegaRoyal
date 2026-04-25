using System.ComponentModel.DataAnnotations;

namespace KeycloakAuth.DTOs;

/// <summary>
/// Payload for registering a new user via Keycloak.
/// After Keycloak registration, the profile is synced to the local database.
/// </summary>
public class RegisterUserDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Payload for local profile sync after Keycloak registration.</summary>
public class SyncUserProfileDto
{
    [Required]
    public string KeycloakId { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
