namespace KeycloakAuth.DTOs;

/// <summary>DTO returned when a user profile is retrieved or created.</summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
