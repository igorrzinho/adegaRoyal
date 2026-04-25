using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using KeycloakAuth.Extensions;
using KeycloakAuth.Data;
using Microsoft.EntityFrameworkCore;
using KeycloakAuth.Filters;
using KeycloakAuth.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuthSupport(builder.Configuration);

// === DATABASE CONFIGURATION ===
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === CORS CONFIGURATION ===
// Allow React Native emulator and mobile clients to connect
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsPolicyBuilder =>
    {
        corsPolicyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// === CONTROLLERS AND JSON SERIALIZATION ===
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent JSON Object Cycle errors
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Use camelCase for React Native/JavaScript compatibility
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // Serialize enums as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// === DEPENDENCY INJECTION ===

// Auth & User Management
builder.Services.AddScoped<SyncKeycloakUserFilter>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Keycloak:BaseUrl"] ?? "http://localhost:8080");
});
// NOTE: Do not register KeycloakAdminService as Scoped separately when using AddHttpClient<TInterface, TImpl>
// The HttpClient factory handles the lifetime automatically.

// Catalog
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Shopping Cart
builder.Services.AddScoped<ICartService, CartService>();

// Orders & Checkout
builder.Services.AddScoped<IOrderService, OrderService>();

// Payment Gateway (Abacate Pay)
builder.Services.AddScoped<IPaymentService, AbacatePayService>();

// Delivery & OTP
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

// === AUTHENTICATION AND AUTHORIZATION ===
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:ValidIssuer"]
            ?? "http://localhost:8080/realms/auth-demo";
        options.Audience = "account";
        options.RequireHttpsMetadata = false;

        // Map Keycloak realm roles from the JWT 'realm_access.roles' claim
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                if (claimsIdentity == null) return Task.CompletedTask;

                // Keycloak stores realm roles under realm_access.roles
                var realmAccess = context.Principal?.FindFirstValue("realm_access");
                if (realmAccess != null)
                {
                    try
                    {
                        var doc = JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var roles))
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                var roleValue = role.GetString();
                                if (!string.IsNullOrEmpty(roleValue))
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                    catch (JsonException) { /* Ignore malformed claims */ }
                }

                return Task.CompletedTask;
            }
        };
    });

// === TELEMETRY ===
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("AdegaRoyal"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });

// === BUILD APPLICATION ===
WebApplication app = builder.Build();

// === MIDDLEWARE ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// === DIAGNOSTIC ENDPOINTS ===
app.MapGet("/users/me", (ClaimsPrincipal claimsPrincipal) =>
{
    return claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
})
.RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "AdegaRoyal", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();