using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeycloakAuth.Filters;
using KeycloakAuth.Services; 
using System.Security.Claims;

namespace KeycloakAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ServiceFilter(typeof(SyncKeycloakUserFilter))] 
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var user = await userService.GetByIdAsync(userId);
        
        return user is not null ? Ok(user) : NotFound();
    }
}