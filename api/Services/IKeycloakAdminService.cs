using AdegaRoyal.Api.DTOs;

namespace AdegaRoyal.Api.Services;

/// <summary>
/// Manages Keycloak Admin API operations: user creation, role assignment and attribute management.
/// </summary>
public interface IKeycloakAdminService
{
    /// <summary>
    /// Registers a new user in Keycloak and returns the new Keycloak user ID.
    /// The caller is responsible for syncing the profile to the local database.
    /// </summary>
    Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto newUser);

    /// <summary>
    /// Authenticates a user against Keycloak using the Resource Owner Password Credentials flow.
    /// Returns the full token response (access_token, refresh_token, expires_in).
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

    Task AddRoleToUserAsync(string userId, string roleName);
    Task RemoveRoleFromUserAsync(string userId, string roleName);
    Task SetUserAttributeAsync(string userId, string attributeName, string attributeValue);
    Task RemoveAttributeFromUserAsync(string userId, string attributeName);
}
