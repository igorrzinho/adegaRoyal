using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KeycloakAuth.DTOs;

namespace KeycloakAuth.Services;

public class KeycloakAdminService(HttpClient httpClient, IConfiguration config) : IKeycloakAdminService
{
    private readonly string _baseUrl = config["Keycloak:BaseUrl"] ?? "http://localhost:8080";
    private readonly string _realm = config["Keycloak:Realm"] ?? "auth-demo";

    public async Task AddRoleToUserAsync(string userId, string roleName)
    {
        await SetAdminTokenAsync();
        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";
        var role = await GetRealmRoleAsync(roleName);
        var content = new StringContent(JsonSerializer.Serialize(new[] { role }), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
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

    private async Task SetAdminTokenAsync()
    {
        var url = $"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token";
        var data = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", config["Keycloak:ClientId"]! },
            { "client_secret", config["Keycloak:ClientSecret"]! }
        };
        var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(data));
        var json = await response.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(json).RootElement.GetProperty("access_token").GetString();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}