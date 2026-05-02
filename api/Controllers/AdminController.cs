using AdegaRoyal.Api.Data;
using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Entities;
using AdegaRoyal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdegaRoyal.Api.Controllers;

/// <summary>
/// Admin-only endpoints for managing users and their claims.
/// Requires the "Admin" role — use [Authorize(Policy = "AdminOnly")] at the class level.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(
    AppDbContext context,
    IUserService userService) : ControllerBase
{
    // ── List all users ─────────────────────────────────────────────────────────

    /// <summary>Returns every registered user. Admin only.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }

    // ── Manage UserClaims ──────────────────────────────────────────────────────

    /// <summary>Adds a custom claim to a user (e.g., "can_manage_products").</summary>
    [HttpPost("users/{userId:guid}/claims")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddClaim(Guid userId, [FromBody] string claimValue)
    {
        if (!await context.Users.AnyAsync(u => u.Id == userId))
            return NotFound(new { message = "Usuário não encontrado." });

        var claim = new UserClaim { UserId = userId, ClaimValue = claimValue };
        context.UserClaims.Add(claim);
        await context.SaveChangesAsync();

        return Created($"/api/admin/users/{userId}/claims", new { id = claim.Id, claimValue });
    }

    /// <summary>Removes a specific claim from a user.</summary>
    [HttpDelete("users/{userId:guid}/claims/{claimId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveClaim(Guid userId, Guid claimId)
    {
        var claim = await context.UserClaims
            .FirstOrDefaultAsync(c => c.Id == claimId && c.UserId == userId);

        if (claim is null)
            return NotFound(new { message = "Claim não encontrada." });

        context.UserClaims.Remove(claim);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Lists all claims of a user.</summary>
    [HttpGet("users/{userId:guid}/claims")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetClaims(Guid userId)
    {
        var claims = await context.UserClaims
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new { c.Id, c.ClaimValue })
            .ToListAsync();

        return Ok(claims);
    }
}
