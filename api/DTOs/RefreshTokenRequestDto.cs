using System.ComponentModel.DataAnnotations;

namespace AdegaRoyal.Api.DTOs;

/// <summary>Request body for the POST /api/auth/refresh endpoint.</summary>
public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
