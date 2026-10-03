using System.Text.Json;

namespace Crm.Api.Middleware;

/// <summary>
/// Middleware that intercepts OpenAPI JSON responses and injects Bearer security scheme.
/// This is a workaround for Microsoft.OpenApi 2.0.0 breaking changes in .NET 10 Preview.
/// </summary>
public class OpenApiSecurityMiddleware
{
    private readonly RequestDelegate _next;

    public OpenApiSecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only intercept OpenAPI document requests
        if (context.Request.Path.StartsWithSegments("/openapi") && 
            context.Request.Path.Value?.EndsWith(".json") == true)
        {
            // Capture the original response
            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            // Read the response
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            // Parse and modify the OpenAPI JSON
            if (!string.IsNullOrEmpty(responseBody) && context.Response.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(responseBody);
                    var root = jsonDoc.RootElement;

                    // Create modified JSON with security scheme
                    using var outputStream = new MemoryStream();
                    using (var writer = new Utf8JsonWriter(outputStream, new JsonWriterOptions { Indented = true }))
                    {
                        WriteWithSecurityScheme(writer, root);
                    }

                    outputStream.Seek(0, SeekOrigin.Begin);
                    var modifiedJson = await new StreamReader(outputStream).ReadToEndAsync();

                    // Write modified response
                    context.Response.Body = originalBody;
                    context.Response.ContentLength = null;
                    await context.Response.WriteAsync(modifiedJson);
                    return;
                }
                catch
                {
                    // If modification fails, return original
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    context.Response.Body = originalBody;
                    await memoryStream.CopyToAsync(originalBody);
                    return;
                }
            }

            // Not JSON or empty, just copy original
            memoryStream.Seek(0, SeekOrigin.Begin);
            context.Response.Body = originalBody;
            await memoryStream.CopyToAsync(originalBody);
        }
        else
        {
            await _next(context);
        }
    }

    private static void WriteWithSecurityScheme(Utf8JsonWriter writer, JsonElement element)
    {
        writer.WriteStartObject();

        bool hasComponents = false;

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Name == "components")
            {
                hasComponents = true;
                WriteComponentsWithSecurity(writer, prop.Value);
            }
            else
            {
                prop.WriteTo(writer);
            }
        }

        // Add components with security if not present
        if (!hasComponents)
        {
            writer.WriteStartObject("components");
            WriteSecuritySchemes(writer);
            writer.WriteEndObject();
        }

        // Add global security requirement
        writer.WriteStartArray("security");
        writer.WriteStartObject();
        writer.WriteStartArray("Bearer");
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    private static void WriteComponentsWithSecurity(Utf8JsonWriter writer, JsonElement components)
    {
        writer.WriteStartObject("components");

        bool hasSecuritySchemes = false;

        foreach (var prop in components.EnumerateObject())
        {
            if (prop.Name == "securitySchemes")
            {
                hasSecuritySchemes = true;
                writer.WriteStartObject("securitySchemes");
                
                // Write existing schemes
                foreach (var scheme in prop.Value.EnumerateObject())
                {
                    scheme.WriteTo(writer);
                }
                
                // Add Bearer if not present
                if (!prop.Value.TryGetProperty("Bearer", out _))
                {
                    WriteBearerScheme(writer);
                }
                
                writer.WriteEndObject();
            }
            else
            {
                prop.WriteTo(writer);
            }
        }

        // Add securitySchemes if not present
        if (!hasSecuritySchemes)
        {
            WriteSecuritySchemes(writer);
        }

        writer.WriteEndObject();
    }

    private static void WriteSecuritySchemes(Utf8JsonWriter writer)
    {
        writer.WriteStartObject("securitySchemes");
        WriteBearerScheme(writer);
        writer.WriteEndObject();
    }

    private static void WriteBearerScheme(Utf8JsonWriter writer)
    {
        writer.WriteStartObject("Bearer");
        writer.WriteString("type", "http");
        writer.WriteString("scheme", "bearer");
        writer.WriteString("bearerFormat", "JWT");
        writer.WriteString("description", "Enter your JWT token from /api/v1/auth/login");
        writer.WriteEndObject();
    }
}

/// <summary>
/// Extension method to register the OpenAPI security middleware.
/// </summary>
public static class OpenApiSecurityMiddlewareExtensions
{
    public static IApplicationBuilder UseOpenApiSecurity(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OpenApiSecurityMiddleware>();
    }
}
