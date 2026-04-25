using KeycloakAuth.DTOs;
using KeycloakAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakAuth.Controllers;

/// <summary>
/// Handles authentication and user registration flows.
/// Registration endpoints are intentionally public (no JWT required).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IKeycloakAdminService keycloakAdmin,
    IUserService userService) : ControllerBase
{
    // ─── Authentication ───────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates a user via Keycloak and returns a JWT access token.
    /// Uses the Resource Owner Password Credentials (ROPC) grant.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
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

    // ─── Registration (Public — no JWT required) ──────────────────────────────

    /// <summary>
    /// Registers a new CUSTOMER in Keycloak and syncs the profile to the local database.
    /// This endpoint is public — no authentication required.
    /// </summary>
    /// <remarks>
    /// Flow:
    /// 1. Creates the user in Keycloak with the <c>customer</c> realm role.
    /// 2. Syncs the profile to the local SQL Server database.
    /// </remarks>
    [HttpPost("register/customer")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<UserDto>> RegisterCustomer([FromBody] RegisterUserDto dto)
    {
        return await RegisterUserWithRole(dto, role: "customer");
    }

    /// <summary>
    /// Registers a new ADMIN in Keycloak and syncs the profile to the local database.
    /// This endpoint is public — no authentication required.
    /// </summary>
    /// <remarks>
    /// In production, protect this endpoint (e.g., require an API key or restrict by network).
    /// Flow:
    /// 1. Creates the user in Keycloak with the <c>admin</c> realm role.
    /// 2. Syncs the profile to the local SQL Server database with Role = Admin.
    /// </remarks>
    [HttpPost("register/admin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<UserDto>> RegisterAdmin([FromBody] RegisterUserDto dto)
    {
        return await RegisterUserWithRole(dto, role: "admin");
    }

    // ─── Profile lookup ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns the local profile of a registered user by their database ID.
    /// Used internally after registration to build the CreatedAtAction location header.
    /// </summary>
    [HttpGet("users/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetProfile(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // ─── Shared logic ─────────────────────────────────────────────────────────

    private async Task<ActionResult<UserDto>> RegisterUserWithRole(RegisterUserDto dto, string role)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string? keycloakUserId = null;

        try
        {
            // Step 1: Create user in Keycloak with the given role
            var keycloakUser = new CreateUserDto
            {
                Username = dto.Email,
                Email    = dto.Email,
                Password = dto.Password,
                Role     = role
            };

            var keycloakResponse = await keycloakAdmin.CreateUserAsync(keycloakUser);
            keycloakUserId = keycloakResponse.UserId;

            // Step 2: Sync local profile to SQL Server
            var syncDto = new SyncUserProfileDto
            {
                KeycloakId = keycloakUserId,
                Name       = dto.Name,
                Email      = dto.Email
            };

            var userProfile = await userService.SyncProfileAsync(syncDto);

            // Step 3: Persist the correct role in the local DB
            if (role == "admin")
                await userService.SetRoleAsync(userProfile.Id, Enums.UserRole.Admin);

            var finalProfile = await userService.GetByIdAsync(userProfile.Id);
            return CreatedAtAction(nameof(GetProfile), new { id = userProfile.Id }, finalProfile);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return Conflict(new { message = "An account with this email already exists in Keycloak." });
        }
        catch (InvalidOperationException ex)
        {
            // Keycloak-side errors (bad config, role not found, etc.)
            return BadRequest(new
            {
                message      = ex.Message,
                keycloakUser = keycloakUserId,
                hint         = keycloakUserId != null
                    ? "User was created in Keycloak but the local DB sync failed. Run POST /api/auth/sync to retry."
                    : null
            });
        }
        catch (Exception ex)
        {
            // Database or unexpected errors — surface the full message
            return StatusCode(500, new
            {
                message      = ex.Message,
                type         = ex.GetType().Name,
                keycloakUser = keycloakUserId,
                hint         = keycloakUserId != null
                    ? "User was created in Keycloak but the local DB sync failed. Ensure migrations are applied: `dotnet ef database update`."
                    : "Registration failed before reaching Keycloak."
            });
        }
    }
}

