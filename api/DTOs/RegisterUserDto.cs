using System.ComponentModel.DataAnnotations;

namespace AdegaRoyal.Api.DTOs;

/// <summary>Payload for registering a new user.</summary>
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
