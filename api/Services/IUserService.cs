using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Enums;

namespace AdegaRoyal.Api.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);

    Task<UserDto?> GetByEmailAsync(string email);

    Task<IEnumerable<UserDto>> GetAllAsync();

    Task SetRoleAsync(Guid userId, UserRole role);
}
