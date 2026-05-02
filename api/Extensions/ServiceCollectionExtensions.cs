using AdegaRoyal.Api.Filters;
using Microsoft.OpenApi;

namespace AdegaRoyal.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuthSupport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo { Title = "Adega Royal API", Version = "v1" });

            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "Bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Informe o JWT obtido em /api/auth/login.\nExemplo: Bearer eyJhbGci..."
            });

            o.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });

            o.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        return services;
    }
}
