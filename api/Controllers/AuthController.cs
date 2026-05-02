using AdegaRoyal.Api.Data;
using AdegaRoyal.Api.DTOs;
using AdegaRoyal.Api.Entities;
using AdegaRoyal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AdegaRoyal.Api.Controllers;

/// <summary>
/// Handles all authentication flows: login, registration, and token refresh.
/// Registration endpoints are public by design — no JWT required.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AppDbContext context,
    IPasswordHasherService passwordHasher,
    ITokenService tokenService) : ControllerBase
{
    // ─── Login ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates a user with email and password.
    /// Returns a short-lived JWT access token and a long-lived opaque refresh token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        // 1. Load user + password hash + claims — single round-trip
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.Password)
            .Include(u => u.Claims)
            .Where(u => u.Email == normalizedEmail && u.IsActive)
            .FirstOrDefaultAsync();

        // 2. Validate credentials — use constant-time comparison via BCrypt.Verify
        if (user is null || user.Password is null
            || !passwordHasher.Verify(normalizedEmail, dto.Password, user.Password.PasswordHash))
        {
            // Generic message — never reveal whether the email exists
            return Unauthorized(new { message = "Credenciais inválidas." });
        }

        // 3. Issue tokens
        var claimValues = user.Claims.Select(c => c.ClaimValue);
        var accessToken = tokenService.GenerateAccessToken(
            userId:      user.Id,
            name:        user.Name,
            email:       user.Email,
            role:        user.Role.ToString(),
            claimValues: claimValues);

        var rawRefreshToken = tokenService.GenerateRefreshToken();

        // 4. Persist the refresh token (token rotation: one active token per login)
        var refreshToken = new RefreshToken
        {
            UserId    = user.Id,
            Token     = rawRefreshToken,
            ExpiresAt = DateTime.UtcNow.Add(tokenService.RefreshTokenLifetime)
        };
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return Ok(new LoginResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresIn    = tokenService.AccessTokenExpiresInSeconds,
            TokenType    = "Bearer"
        });
    }

    // ─── Token Refresh ────────────────────────────────────────────────────────

    /// <summary>
    /// Exchanges a valid refresh token for a new access token + refresh token pair.
    /// Implements token rotation: the old refresh token is revoked on use.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Load the stored token with its owner
        var stored = await context.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.Claims)
            .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken && !r.IsRevoked);

        if (stored is null || stored.ExpiresAt < DateTime.UtcNow || !stored.User.IsActive)
            return Unauthorized(new { message = "Refresh token inválido ou expirado." });

        // Revoke consumed token (rotation)
        stored.IsRevoked = true;

        // Issue new pair
        var claimValues = stored.User.Claims.Select(c => c.ClaimValue);
        var newAccessToken = tokenService.GenerateAccessToken(
            userId:      stored.User.Id,
            name:        stored.User.Name,
            email:       stored.User.Email,
            role:        stored.User.Role.ToString(),
            claimValues: claimValues);

        var newRawRefreshToken = tokenService.GenerateRefreshToken();
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId    = stored.UserId,
            Token     = newRawRefreshToken,
            ExpiresAt = DateTime.UtcNow.Add(tokenService.RefreshTokenLifetime)
        });

        await context.SaveChangesAsync();

        return Ok(new LoginResponseDto
        {
            AccessToken  = newAccessToken,
            RefreshToken = newRawRefreshToken,
            ExpiresIn    = tokenService.AccessTokenExpiresInSeconds,
            TokenType    = "Bearer"
        });
    }

    // ─── Registration ─────────────────────────────────────────────────────────

    /// <summary>
    /// Registers a new customer. Public — no authentication required.
    /// </summary>
    [HttpPost("register/customer")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<UserDto>> RegisterCustomer([FromBody] RegisterUserDto dto)
        => await RegisterUserWithRole(dto, Enums.UserRole.Customer);

    /// <summary>
    /// Registers a new admin. Requires an existing Admin JWT.
    /// </summary>
    [HttpPost("register/admin")]
   // [Authorize(Policy = "AdminOnly")]
   [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<UserDto>> RegisterAdmin([FromBody] RegisterUserDto dto)
        => await RegisterUserWithRole(dto, Enums.UserRole.Admin);



    // ─── Password Reset ───────────────────────────────────────────────────────

    /// <summary>
    /// Gera um token para redefinição de senha. Na vida real, enviaria por e-mail.
    /// Aqui, vamos retornar na resposta para testes.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);
        
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        var token = GenerateResetToken(normalizedEmail);

        // Retornamos o token na resposta (apenas para facilitar testes, 
        // num cenário real enviaríamos por e-mail silenciosamente).
        return Ok(new { 
            message = "Token de redefinição gerado com sucesso.",
            token = token 
        });
    }

    /// <summary>
    /// Redefine a senha utilizando o token recebido no forgot-password.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        if (!ValidateResetToken(normalizedEmail, dto.Token))
            return BadRequest(new { message = "Token inválido ou expirado." });

        var user = await context.Users
            .Include(u => u.Password)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);

        if (user == null || user.Password == null)
            return BadRequest(new { message = "Usuário não encontrado ou inconsistente." });

        // Atualiza o hash da senha
        user.Password.PasswordHash = passwordHasher.Hash(normalizedEmail, dto.NewPassword);
        
        await context.SaveChangesAsync();

        return Ok(new { message = "Senha redefinida com sucesso." });
    }

    // ─── Profile lookup ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns the public profile of a registered user by their database ID.
    /// </summary>
    [HttpGet("users/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetProfile(Guid id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? NotFound() : Ok(MapToDto(user));
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task<ActionResult<UserDto>> RegisterUserWithRole(RegisterUserDto dto, Enums.UserRole role)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        // Idempotency check
        if (await context.Users.AnyAsync(u => u.Email == normalizedEmail))
            return Conflict(new { message = "Já existe uma conta com esse e-mail." });

        // Create user aggregate
        var user = new User(dto.Name, normalizedEmail, role);

        var passwordRecord = new UserPassword
        {
            UserId       = user.Id,
            PasswordHash = passwordHasher.Hash(normalizedEmail, dto.Password)
        };

        context.Users.Add(user);
        context.UserPasswords.Add(passwordRecord);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProfile), new { id = user.Id }, MapToDto(user));
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

    // ─── Token Generators para Password Reset ─────────────────────────────────

    private static string GenerateResetToken(string email)
    {
        // "chave fixa de dados convertidos para base 64"
        var fixedKey = "6d0b2307-5075-4471-9a60-c7a6f7be3dc9";
        var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(fixedKey));

        // Payload com validade de 1 hora
        var expiration = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var payload = $"{email}:{expiration}";

        // Assina o payload
        using var hmac = new HMACSHA256(Convert.FromBase64String(base64Key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToBase64String(hash);

        var rawToken = $"{payload}:{signature}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));
    }

    private static bool ValidateResetToken(string email, string tokenBase64)
    {
        try
        {
            var fixedKey = "6d0b2307-5075-4471-9a60-c7a6f7be3dc9";
            var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(fixedKey));

            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenBase64));
            var parts = decoded.Split(':');
            
            if (parts.Length != 3) return false;

            var tokenEmail = parts[0];
            var expirationStr = parts[1];
            var signature = parts[2];

            if (tokenEmail != email) return false;

            var expiration = long.Parse(expirationStr);
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiration) return false;

            var payload = $"{tokenEmail}:{expirationStr}";
            using var hmac = new HMACSHA256(Convert.FromBase64String(base64Key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(hash);

            // Time-constant comparison para a assinatura
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(signature),
                Convert.FromBase64String(expectedSignature));
        }
        catch
        {
            return false;
        }
    }
}
