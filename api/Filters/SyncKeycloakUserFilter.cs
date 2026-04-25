using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using KeycloakAuth.Data;
using KeycloakAuth.Services;

namespace KeycloakAuth.Filters;

/// <summary>
/// Action filter that syncs the Keycloak user profile to the local database on each authenticated request.
/// Delegates to IUserService.SyncProfileAsync for consistency.
/// </summary>
public class SyncKeycloakUserFilter(AppDbContext dbContext, IUserService userService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var principal = context.HttpContext.User;

        if (principal.Identity is { IsAuthenticated: true })
        {
            // Keycloak 'sub' claim is the canonical user identifier
            var keycloakId = principal.FindFirst("sub")?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(keycloakId))
            {
                var name = principal.FindFirst("name")?.Value
                           ?? principal.FindFirst("preferred_username")?.Value
                           ?? "Keycloak User";

                var email = principal.FindFirst(ClaimTypes.Email)?.Value
                            ?? principal.FindFirst("email")?.Value
                            ?? $"{keycloakId}@no-email.local";

                var existing = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

                if (existing == null || existing.Name != name || existing.Email != email)
                {
                    await userService.SyncProfileAsync(new DTOs.SyncUserProfileDto
                    {
                        KeycloakId = keycloakId,
                        Name = name,
                        Email = email
                    });
                }
            }
        }

        await next();
    }
}