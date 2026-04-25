using KeycloakAuth.Enums;

namespace KeycloakAuth.Entities;

/// <summary>
/// Represents a registered user in the Adega Royal system, synchronized from Keycloak.
/// </summary>
public class User(string keycloakId, string name, string email, UserRole role = UserRole.Customer)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The subject ID from Keycloak JWT ('sub' claim).</summary>
    public string KeycloakId { get; set; } = keycloakId;

    public string Name { get; set; } = name;
    public string Email { get; set; } = email;
    public UserRole Role { get; set; } = role;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}