using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
namespace WebApi.Swagger.Headers
{
    //https://stackoverflow.com/questions/41493130/web-api-how-to-add-a-header-parameter-for-all-api-in-swagger
    //https://github.com/domaindrivendev/Swashbuckle/issues/501#issuecomment-143254123
    //https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1197
    public class AddAppNameRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        { 
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "AppName",
                In = ParameterLocation.Header, 
                Description = "Calling application name",
                Required = true,
                 AllowEmptyValue = false
            });
        }
    }
}
