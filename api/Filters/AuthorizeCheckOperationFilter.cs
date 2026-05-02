using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdegaRoyal.Api.Filters;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var classHasAllowAnonymous = context.MethodInfo.DeclaringType?
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any() ?? false;

        var methodHasAllowAnonymous = context.MethodInfo
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();

        var isAnonymous = classHasAllowAnonymous || methodHasAllowAnonymous;

        if (isAnonymous)
        {
            operation.Security = new List<OpenApiSecurityRequirement>();
        }

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Não autenticado." });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Sem permissão." });
    }
}
