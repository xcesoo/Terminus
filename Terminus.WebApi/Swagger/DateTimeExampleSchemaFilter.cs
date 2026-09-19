using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Terminus.WebApi.Swagger;

/// <summary>
/// Проставляє для будь-якого параметра/поля типу DateTime готовий приклад
/// </summary>
public class DateTimeExampleSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concreteSchema) return;
        if (context.Type != typeof(DateTime) && context.Type != typeof(DateTime?)) return;

        var example = DateTime.UtcNow.Date.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
        concreteSchema.Example = JsonValue.Create(example);
        concreteSchema.Description = string.IsNullOrWhiteSpace(concreteSchema.Description)
            ? $"Формат: ISO 8601, UTC (суфікс Z). Приклад: {example}"
            : concreteSchema.Description; 
    }
}
