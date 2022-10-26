/*
    Manuka Yasas (IT19133850)
    SecureEndpointAuthRequirementFilter will
    enable users to authenticate using bearer token through the swagger ui 
*/
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class SecureEndpointAuthRequirementFilter : IOperationFilter
{
    // enable users to authenticate using bearer token through the swagger ui 
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.ApiDescription
            .ActionDescriptor
            .EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .Any())
        {
            return;
        }

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "token" }
                }] = new List<string>()
            }
        };
    }
}