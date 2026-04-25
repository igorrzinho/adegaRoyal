using KeycloakAuth.DTOs;
using KeycloakAuth.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakAuth.Controllers;

/// <summary>
/// Handles Keycloak authentication flows: login and user registration with local profile sync.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IKeycloakAdminService keycloakAdmin,
    IUserService userService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user via Keycloak and returns a JWT access token.
    /// </summary>
    /// <remarks>
    /// Uses the Keycloak Resource Owner Password Credentials (ROPC) grant.
    /// The returned access_token is used as Bearer token for all subsequent API calls.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await keycloakAdmin.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registers a new customer in Keycloak and synchronizes the profile to the local database.
    /// </summary>
    /// <remarks>
    /// Flow:
    /// 1. Creates the user in Keycloak with the 'customer' role.
    /// 2. Logs in immediately to retrieve the Keycloak user ID from the JWT.
    /// 3. Syncs the profile to the local SQL Server database.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Step 1: Create user in Keycloak
            var keycloakUser = new CreateUserDto
            {
                Username = dto.Email,
                Email = dto.Email,
                Password = dto.Password,
                Role = "customer"
            };

            var keycloakResponse = await keycloakAdmin.CreateUserAsync(keycloakUser);

            // Step 2: Sync local profile using Keycloak ID
            var syncDto = new SyncUserProfileDto
            {
                KeycloakId = keycloakResponse.UserId,
                Name = dto.Name,
                Email = dto.Email
            };

            var userProfile = await userService.SyncProfileAsync(syncDto);

            return CreatedAtAction(nameof(GetProfile), new { id = userProfile.Id }, userProfile);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns the local profile of a registered user by their database ID.
    /// </summary>
    [HttpGet("users/{id:guid}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetProfile(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
