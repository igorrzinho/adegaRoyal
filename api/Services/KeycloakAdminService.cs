using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AdegaRoyal.Api.DTOs;

namespace AdegaRoyal.Api.Services;

public class KeycloakAdminService(HttpClient httpClient, IConfiguration config) : IKeycloakAdminService
{
    private readonly string _baseUrl = config["Keycloak:BaseUrl"] ?? "http://localhost:8080";
    private readonly string _realm = config["Keycloak:Realm"] ?? "adega-royal";

    public async Task AddRoleToUserAsync(string userId, string roleName)
    {
        await SetAdminTokenAsync();
        
        // First, ensure the role exists
        await EnsureRealmRoleExistsAsync(roleName);
        
        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";
        var role = await GetRealmRoleAsync(roleName);
        var content = new StringContent(JsonSerializer.Serialize(new[] { role }), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to add role '{roleName}' to user '{userId}': {response.StatusCode} - {error}");
        }
    }

    public async Task RemoveRoleFromUserAsync(string userId, string roleName)
    {
        await SetAdminTokenAsync();
        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";
        var role = await GetRealmRoleAsync(roleName);

        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(new[] { role }), Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetUserAttributeAsync(string userId, string attributeName, string attributeValue)
    {
        await SetAdminTokenAsync();
        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";
        var updateData = new { attributes = new Dictionary<string, string[]> { { attributeName, new[] { attributeValue } } } };
        var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAttributeFromUserAsync(string userId, string attributeName)
    {
        await SetAdminTokenAsync();
        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";
        var updateData = new { attributes = new Dictionary<string, string[]> { { attributeName, Array.Empty<string>() } } };
        var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto newUser)
    {
        await SetAdminTokenAsync();

        var createUserUrl = $"{_baseUrl}/admin/realms/{_realm}/users";
        var userPayload = new
        {
            username = newUser.Username,
            email = newUser.Email,
            enabled = true,
            emailVerified = true
        };

        var createResponse = await httpClient.PostAsync(createUserUrl,
            new StringContent(JsonSerializer.Serialize(userPayload), Encoding.UTF8, "application/json"));
        createResponse.EnsureSuccessStatusCode();

        var userId = ExtractIdFromLocation(createResponse.Headers.Location);
        await SetUserPasswordAsync(userId, newUser.Password);
        await AddRoleToUserAsync(userId, newUser.Role);

        return new CreateUserResponseDto
        {
            UserId = userId,
            Username = newUser.Username,
            Email = newUser.Email,
            Role = newUser.Role,
            Location = $"/api/admin/users/{userId}"
        };
    }

    /// <summary>
    /// Authenticates via Keycloak Resource Owner Password Credentials grant.
    /// Returns tokens for the mobile/web client.
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var url = $"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", config["Keycloak:PublicClientId"] ?? "public-client" },
            { "username", dto.Email },
            { "password", dto.Password }
        };

        var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(data));

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new UnauthorizedAccessException($"Authentication failed: {errorBody}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json).RootElement;

        return new LoginResponseDto
        {
            AccessToken = doc.GetProperty("access_token").GetString() ?? string.Empty,
            RefreshToken = doc.TryGetProperty("refresh_token", out var rt) ? rt.GetString() ?? string.Empty : string.Empty,
            ExpiresIn = doc.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 300,
            TokenType = "Bearer"
        };
    }

    private async Task<JsonElement> GetRealmRoleAsync(string roleName)
    {
        var url = $"{_baseUrl}/admin/realms/{_realm}/roles/{roleName}";
        var response = await httpClient.GetAsync(url);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found in realm '{_realm}'");
        }
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private async Task SetUserPasswordAsync(string userId, string password)
    {
        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/reset-password";
        var passwordPayload = new
        {
            type = "password",
            value = password,
            temporary = false
        };

        var response = await httpClient.PutAsync(url,
            new StringContent(JsonSerializer.Serialize(passwordPayload), Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    private static string ExtractIdFromLocation(Uri? location)
    {
        if (location == null || !location.Segments.Any())
        {
            throw new InvalidOperationException("Não foi possível obter o ID do usuário criado a partir do cabeçalho Location do Keycloak.");
        }

        return location.Segments.Last().TrimEnd('/');
    }

    /// <summary>
    /// Obtains an admin access token from Keycloak using the master realm admin credentials.
    /// Uses the password grant with the built-in admin-cli public client — no secret required.
    /// Credentials come from Keycloak:AdminUsername / Keycloak:AdminPassword in appsettings.
    /// </summary>
    private async Task SetAdminTokenAsync()
    {
        var adminUsername = config["Keycloak:AdminUsername"]
            ?? throw new InvalidOperationException("Keycloak:AdminUsername is not configured.");
        var adminPassword = config["Keycloak:AdminPassword"]
            ?? throw new InvalidOperationException("Keycloak:AdminPassword is not configured.");

        // Always use the master realm to obtain the admin token
        var url = $"{_baseUrl}/realms/master/protocol/openid-connect/token";

        var data = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id",  "admin-cli" },
            { "username",   adminUsername },
            { "password",   adminPassword }
        };

        var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(data));
        var json     = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Keycloak admin token request failed ({(int)response.StatusCode}): {json}");
        }

        var doc = JsonDocument.Parse(json).RootElement;

        if (!doc.TryGetProperty("access_token", out var tokenElement) ||
            string.IsNullOrEmpty(tokenElement.GetString()))
        {
            throw new InvalidOperationException(
                $"Keycloak did not return an access_token. Response: {json}");
        }

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenElement.GetString());
    }

    private async Task EnsureRealmRoleExistsAsync(string roleName)
    {
        try
        {
            await GetRealmRoleAsync(roleName);
            // Role exists, no need to create
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            // Role doesn't exist, create it
            var createRoleUrl = $"{_baseUrl}/admin/realms/{_realm}/roles";
            var rolePayload = new
            {
                name = roleName,
                description = $"{roleName} role for Adega Royal"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(rolePayload), 
                Encoding.UTF8, 
                "application/json");
            
            var response = await httpClient.PostAsync(createRoleUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {response.StatusCode} - {error}");
            }
        }
    }
}
