using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeycloakAuth.DTOs;
using KeycloakAuth.Services;

namespace KeycloakAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AdminController(IKeycloakAdminService adminService) : ControllerBase
{
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUser)
    {
        if (newUser is null)
        {
            return BadRequest(new { message = "Dados do usuário são obrigatórios." });
        }

        var role = newUser.Role?.Trim();
        if (string.IsNullOrWhiteSpace(role) || !(role.Equals("admin", StringComparison.OrdinalIgnoreCase) || role.Equals("user", StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { message = "Role deve ser 'admin' ou 'user'." });
        }

        var result = await adminService.CreateUserAsync(newUser);
        return Created(result.Location, result);
    }

    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] string roleName)
    {
        await adminService.AddRoleToUserAsync(userId, roleName);
        return Ok(new { message = "Role adicionada" });
    }

    [HttpDelete("users/{userId}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        await adminService.RemoveRoleFromUserAsync(userId, roleName);
        return Ok(new { message = "Role removida" });
    }

    [HttpPost("users/{userId}/claims")]
    public async Task<IActionResult> SetClaim(string userId, string key, string value)
    {
        await adminService.SetUserAttributeAsync(userId, key, value);
        return Ok(new { message = "Atributo definido" });
    }

    [HttpDelete("users/{userId}/claims/{key}")]
    public async Task<IActionResult> RemoveClaim(string userId, string key)
    {
        await adminService.RemoveAttributeFromUserAsync(userId, key);
        return Ok(new { message = "Atributo removido" });
    }
}