using KeycloakAuth.DTOs;
using KeycloakAuth.Entities;
using KeycloakAuth.Enums;

namespace KeycloakAuth.Services;

/// <summary>
/// Manages local user profiles that are synchronized from Keycloak after registration.
/// </summary>
public interface IUserService
{
    /// <summary>Finds a user by their Keycloak subject ID (JWT 'sub' claim).</summary>
    Task<UserDto?> GetByKeycloakIdAsync(string keycloakId);

    /// <summary>Finds a user by their local database ID.</summary>
    Task<UserDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates or updates a user profile from Keycloak data.
    /// Called automatically after Keycloak registration.
    /// </summary>
    Task<UserDto> SyncProfileAsync(SyncUserProfileDto dto);

    /// <summary>Returns all users (admin only).</summary>
    Task<IEnumerable<UserDto>> GetAllAsync();
}