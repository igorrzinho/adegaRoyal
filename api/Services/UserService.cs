using KeycloakAuth.Data;
using KeycloakAuth.DTOs;
using KeycloakAuth.Entities;
using KeycloakAuth.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeycloakAuth.Services;

/// <summary>
/// Manages local user profiles synchronized from Keycloak after registration.
/// Uses primary constructor for dependency injection.
/// </summary>
public class UserService(AppDbContext context) : IUserService
{
    public async Task<UserDto?> GetByKeycloakIdAsync(string keycloakId)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> SyncProfileAsync(SyncUserProfileDto dto)
    {
        var existing = await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == dto.KeycloakId);

        if (existing != null)
        {
            // Update profile data on re-sync
            existing.Name = dto.Name;
            existing.Email = dto.Email;
            await context.SaveChangesAsync();
            return MapToDto(existing);
        }

        // First time: create new local profile
        var user = new User(dto.KeycloakId, dto.Name, dto.Email);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => MapToDto(u))
            .ToListAsync();
    }

    public async Task SetRoleAsync(Guid userId, UserRole role)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) return;

        user.Role = role;
        await context.SaveChangesAsync();
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        KeycloakId = user.KeycloakId,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role.ToString(),
        CreatedAt = user.CreatedAt,
        IsActive = user.IsActive
    };
}
