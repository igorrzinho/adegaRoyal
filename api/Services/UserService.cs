using AdegaRoyal.Api.Data;
using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace AdegaRoyal.Api.Services;

public class UserService(AppDbContext context) : IUserService
{
    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var normalized = email.ToLowerInvariant();
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized);

        return user is null ? null : MapToDto(user);
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
        if (user is null) return;

        user.Role = role;
        await context.SaveChangesAsync();
    }

    private static UserDto MapToDto(Entities.User user) => new()
    {
        Id        = user.Id,
        Name      = user.Name,
        Email     = user.Email,
        Role      = user.Role.ToString(),
        CreatedAt = user.CreatedAt,
        IsActive  = user.IsActive
    };
}
