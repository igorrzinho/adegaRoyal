using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdegaRoyal.Api.Services;
using System.Security.Claims;

namespace AdegaRoyal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
